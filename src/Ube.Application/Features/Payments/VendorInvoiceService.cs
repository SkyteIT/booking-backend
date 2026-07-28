using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

// Collects money the VENDOR owes the PLATFORM for VendorCollected bookings
// (cash/card taken by the vendor directly) - the direction PayoutBatch
// doesn't cover. v1 approach: manual invoice + suspend-if-unpaid, not an
// automated card-on-file charge (that depends on the eventual gateway
// supporting saved payment methods, which isn't known yet).
public class VendorInvoiceService : IVendorInvoiceService
{
    private readonly IVendorInvoiceRepository _invoiceRepo;
    private readonly IPayoutBatchRepository _batchRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public VendorInvoiceService(
        IVendorInvoiceRepository invoiceRepo,
        IPayoutBatchRepository batchRepo,
        ILedgerRepository ledgerRepo,
        IVendorProfileRepository vendorRepo,
        IPaymentAuditLogRepository auditRepo)
    {
        _invoiceRepo = invoiceRepo;
        _batchRepo = batchRepo;
        _ledgerRepo = ledgerRepo;
        _vendorRepo = vendorRepo;
        _auditRepo = auditRepo;
    }

    public async Task<VendorInvoiceDto> ComputeAsync(ComputeVendorInvoiceRequest request, CancellationToken ct = default)
    {
        var existingInvoices = await _invoiceRepo.GetByVendorIdAsync(request.VendorProfileId, ct);
        if (existingInvoices.Any(i => i.Status is VendorInvoiceStatus.Pending or VendorInvoiceStatus.Overdue))
            throw new BusinessRuleException("This vendor already has an unresolved commission invoice");

        var existingBatches = await _batchRepo.GetByVendorIdAsync(request.VendorProfileId, ct);
        if (existingBatches.Any(b => b.Status != PayoutBatchStatus.Settled))
            throw new BusinessRuleException("This vendor has an unresolved payout batch - resolve it before computing an invoice");

        var entries = await _ledgerRepo.GetByVendorIdAsOfAsync(request.VendorProfileId, request.PeriodEnd, ct);
        var vendorEntries = entries.Where(e => e.AccountType == LedgerAccountType.Vendor);

        var balance = vendorEntries.Sum(e => e.Direction == LedgerDirection.Credit ? e.Amount : -e.Amount);
        if (balance >= 0)
            throw new BusinessRuleException("This vendor does not owe the platform anything for the given period");

        var invoice = new VendorCommissionInvoice
        {
            Id = Guid.NewGuid(),
            VendorProfileId = request.VendorProfileId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            AmountOwed = -balance,
            DueDate = request.PeriodEnd.AddDays(request.GracePeriodDays),
            Status = VendorInvoiceStatus.Pending
        };

        await _invoiceRepo.AddAsync(invoice, ct);
        return ToDto(invoice);
    }

    public async Task<VendorInvoiceDto> MarkPaidAsync(Guid actorUserId, Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException("Invoice not found");

        if (invoice.Status is not (VendorInvoiceStatus.Pending or VendorInvoiceStatus.Overdue))
            throw new BusinessRuleException("Only a pending or overdue invoice can be marked paid");

        invoice.Status = VendorInvoiceStatus.PaidManually;
        invoice.ResolvedByUserId = actorUserId;
        invoice.ResolvedAt = DateTime.UtcNow;
        await _invoiceRepo.UpdateAsync(invoice, ct);

        // Closes the debt out in the running balance - a new Credit entry,
        // never an edit to the original Debit entries that created it.
        await _ledgerRepo.AddAsync(new LedgerEntry
        {
            Id = Guid.NewGuid(),
            AccountType = LedgerAccountType.Vendor,
            VendorProfileId = invoice.VendorProfileId,
            EntryType = LedgerEntryType.InvoicePayment,
            Direction = LedgerDirection.Credit,
            Amount = invoice.AmountOwed,
            VendorCommissionInvoiceId = invoice.Id
        }, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "VendorInvoiceMarkedPaid",
            EntityType = nameof(VendorCommissionInvoice),
            EntityId = invoice.Id
        }, ct);

        return ToDto(invoice);
    }

    public async Task<VendorInvoiceDto> MarkOverdueAsync(Guid actorUserId, Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException("Invoice not found");

        if (invoice.Status != VendorInvoiceStatus.Pending)
            throw new BusinessRuleException("Only a pending invoice can be marked overdue");

        if (invoice.DueDate >= DateTime.UtcNow)
            throw new BusinessRuleException("This invoice is not past its due date yet");

        invoice.Status = VendorInvoiceStatus.Overdue;
        await _invoiceRepo.UpdateAsync(invoice, ct);

        // Enforcement: suspend the vendor's account until they pay. A
        // vendor with no visibility on the platform is a strong incentive
        // to settle; this is the v1 collection mechanism until a real
        // card-on-file auto-debit is possible.
        var vendor = await _vendorRepo.GetByIdAsync(invoice.VendorProfileId);
        if (vendor != null && vendor.IsActive)
        {
            vendor.IsActive = false;
            await _vendorRepo.UpdateAsync(vendor);
        }

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "VendorInvoiceOverdueSuspended",
            EntityType = nameof(VendorCommissionInvoice),
            EntityId = invoice.Id
        }, ct);

        return ToDto(invoice);
    }

    public async Task<IReadOnlyList<VendorInvoiceDto>> ProcessOverdueAsync(Guid actorUserId, CancellationToken ct = default)
    {
        var candidates = await _invoiceRepo.GetOverdueCandidatesAsync(DateTime.UtcNow, ct);
        var results = new List<VendorInvoiceDto>();
        foreach (var candidate in candidates)
            results.Add(await MarkOverdueAsync(actorUserId, candidate.Id, ct));
        return results;
    }

    public async Task<IReadOnlyList<VendorInvoiceDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default)
    {
        var invoices = await _invoiceRepo.GetByVendorIdAsync(vendorProfileId, ct);
        return invoices.Select(ToDto).ToList();
    }

    private static VendorInvoiceDto ToDto(VendorCommissionInvoice i) => new()
    {
        Id = i.Id,
        VendorProfileId = i.VendorProfileId,
        PeriodStart = i.PeriodStart,
        PeriodEnd = i.PeriodEnd,
        AmountOwed = i.AmountOwed,
        DueDate = i.DueDate,
        Status = i.Status,
        ResolvedAt = i.ResolvedAt,
        CreatedAt = i.CreatedAt
    };
}
