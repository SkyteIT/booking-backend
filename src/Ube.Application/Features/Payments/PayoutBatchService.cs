using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PayoutBatchService : IPayoutBatchService
{
    private readonly IPayoutBatchRepository _batchRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;
    private readonly IVendorInvoiceRepository _invoiceRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly INotificationService _notificationService;

    public PayoutBatchService(
        IPayoutBatchRepository batchRepo,
        ILedgerRepository ledgerRepo,
        IPaymentAuditLogRepository auditRepo,
        IVendorInvoiceRepository invoiceRepo,
        IVendorProfileRepository vendorRepo,
        INotificationService notificationService)
    {
        _batchRepo = batchRepo;
        _ledgerRepo = ledgerRepo;
        _auditRepo = auditRepo;
        _invoiceRepo = invoiceRepo;
        _vendorRepo = vendorRepo;
        _notificationService = notificationService;
    }

    public async Task<PayoutBatchDto> ComputeAsync(ComputePayoutBatchRequest request, CancellationToken ct = default)
    {
        // Only one unresolved claim on a vendor's running balance may exist
        // at a time - a batch and an invoice both read the same cumulative
        // ledger sum, so two unresolved claims would double-count whatever
        // hasn't been settled/paid yet.
        var existingBatches = await _batchRepo.GetByVendorIdAsync(request.VendorProfileId, ct);
        if (existingBatches.Any(b => b.Status != PayoutBatchStatus.Settled))
            throw new BusinessRuleException("This vendor already has an unresolved payout batch - settle it before computing a new one");

        var existingInvoices = await _invoiceRepo.GetByVendorIdAsync(request.VendorProfileId, ct);
        if (existingInvoices.Any(i => i.Status is VendorInvoiceStatus.Pending or VendorInvoiceStatus.Overdue))
            throw new BusinessRuleException("This vendor has an unresolved commission invoice - resolve it before computing a payout batch");

        var entries = await _ledgerRepo.GetByVendorIdAsOfAsync(request.VendorProfileId, request.PeriodEnd, ct);
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

        // A new Settlement entry (a Debit) nets the batched amount back to
        // zero in the running balance - tagged with PayoutBatchId at
        // creation, never by editing the entries it covers.
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

        var vendor = await _vendorRepo.GetByIdAsync(batch.VendorProfileId);
        if (vendor != null)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = vendor.UserId,
                    Title = "Payout processed",
                    Message = $"A payout of {batch.TotalAmount:F2} has been settled for {batch.PeriodStart:yyyy-MM-dd} to {batch.PeriodEnd:yyyy-MM-dd}.",
                    Type = (int)NotificationType.PayoutProcessed
                }, ct);
            }
            catch
            {
                // Best-effort - a notification failure never blocks a payout that's already settled.
            }
        }

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
