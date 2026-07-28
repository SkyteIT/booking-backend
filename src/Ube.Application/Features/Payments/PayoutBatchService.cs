using Ube.Application.Common.Exceptions;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PayoutBatchService : IPayoutBatchService
{
    private readonly IPayoutBatchRepository _batchRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public PayoutBatchService(
        IPayoutBatchRepository batchRepo,
        ILedgerRepository ledgerRepo,
        IPaymentAuditLogRepository auditRepo)
    {
        _batchRepo = batchRepo;
        _ledgerRepo = ledgerRepo;
        _auditRepo = auditRepo;
    }

    public async Task<PayoutBatchDto> ComputeAsync(ComputePayoutBatchRequest request, CancellationToken ct = default)
    {
        var existingBatches = await _batchRepo.GetByVendorIdAsync(request.VendorProfileId, ct);
        var overlapping = existingBatches.Any(b =>
            b.Status != PayoutBatchStatus.Settled &&
            b.PeriodStart < request.PeriodEnd && request.PeriodStart < b.PeriodEnd);
        if (overlapping)
            throw new BusinessRuleException("An unsettled batch already covers an overlapping period for this vendor");

        var entries = await _ledgerRepo.GetUnbatchedByVendorIdAsync(request.VendorProfileId, request.PeriodEnd, ct);
        var vendorEntries = entries.Where(e => e.AccountType == LedgerAccountType.Vendor);

        var total = vendorEntries.Sum(e => e.Direction == LedgerDirection.Credit ? e.Amount : -e.Amount);
        if (total <= 0)
            throw new BusinessRuleException("Nothing owed to this vendor for the given period");

        var batch = new PayoutBatch
        {
            Id = Guid.NewGuid(),
            VendorProfileId = request.VendorProfileId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TotalAmount = total,
            Status = PayoutBatchStatus.Pending
        };

        await _batchRepo.AddAsync(batch, ct);
        return ToDto(batch);
    }

    public async Task<PayoutBatchDto> SettleAsync(Guid settledByUserId, Guid batchId, CancellationToken ct = default)
    {
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
            ?? throw new NotFoundException("Payout batch not found");

        if (batch.Status == PayoutBatchStatus.Settled)
            throw new BusinessRuleException("Batch is already settled");

        batch.Status = PayoutBatchStatus.Settled;
        batch.SettledByUserId = settledByUserId;
        batch.SettledAt = DateTime.UtcNow;
        await _batchRepo.UpdateAsync(batch, ct);

        // A new Settlement entry closes out the batched amount - tagged
        // with PayoutBatchId at creation, never by editing the entries it
        // covers. Future "unbatched" queries naturally exclude it.
        await _ledgerRepo.AddAsync(new LedgerEntry
        {
            Id = Guid.NewGuid(),
            AccountType = LedgerAccountType.Vendor,
            VendorProfileId = batch.VendorProfileId,
            EntryType = LedgerEntryType.Settlement,
            Direction = LedgerDirection.Debit,
            Amount = batch.TotalAmount,
            PayoutBatchId = batch.Id
        }, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = settledByUserId,
            Action = "PayoutBatchSettled",
            EntityType = nameof(PayoutBatch),
            EntityId = batch.Id
        }, ct);

        return ToDto(batch);
    }

    public async Task<IReadOnlyList<PayoutBatchDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default)
    {
        var batches = await _batchRepo.GetByVendorIdAsync(vendorProfileId, ct);
        return batches.Select(ToDto).ToList();
    }

    private static PayoutBatchDto ToDto(PayoutBatch b) => new()
    {
        Id = b.Id,
        VendorProfileId = b.VendorProfileId,
        PeriodStart = b.PeriodStart,
        PeriodEnd = b.PeriodEnd,
        TotalAmount = b.TotalAmount,
        Status = b.Status,
        SettledAt = b.SettledAt,
        CreatedAt = b.CreatedAt
    };
}
