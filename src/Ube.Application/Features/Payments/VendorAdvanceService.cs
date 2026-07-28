using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Category;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

// Pays a vendor part of what they're already owed for a booking EARLY,
// ahead of the normal payout batch cycle - not an extra amount on top.
// Modeled as a single Debit against the vendor's ledger balance (same
// pattern as a payout Settlement), so the normal payout cycle later only
// pays out whatever's left, with no separate bookkeeping needed to avoid
// double-paying.
public class VendorAdvanceService : IVendorAdvanceService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;

    public VendorAdvanceService(
        IPaymentRepository paymentRepo,
        IBookingRepository bookingRepo,
        ICategoryRepository categoryRepo,
        ILedgerRepository ledgerRepo,
        IPaymentAuditLogRepository auditRepo)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
        _categoryRepo = categoryRepo;
        _ledgerRepo = ledgerRepo;
        _auditRepo = auditRepo;
    }

    public async Task<VendorAdvanceDto> IssueAdvanceAsync(Guid actorUserId, IssueVendorAdvanceRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.Captured)
            throw new BusinessRuleException("Only a captured payment is eligible for a vendor advance");

        if (payment.CollectionMethod != PaymentCollectionMethod.PlatformCollected)
            throw new BusinessRuleException("Advances only apply when the platform collected the payment - a vendor who already holds the cash directly has nothing to advance");

        var booking = await _bookingRepo.GetByIdAsync(payment.BookingId)
            ?? throw new NotFoundException("Booking not found");

        var category = await _categoryRepo.GetByIdAsync(booking.Listing.CategoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        if (!category.AllowsVendorAdvance || !category.AdvancePercent.HasValue)
            throw new BusinessRuleException("This category does not allow vendor advances");

        var existingEntries = await _ledgerRepo.GetByPaymentIdAsync(payment.Id, ct);
        if (existingEntries.Any(e => e.EntryType == LedgerEntryType.AdvancePayout))
            throw new BusinessRuleException("An advance has already been issued for this payment");

        var amount = MoneyMath.PercentOf(payment.NetVendorAmount, category.AdvancePercent.Value);

        await _ledgerRepo.AddAsync(new LedgerEntry
        {
            Id = Guid.NewGuid(),
            AccountType = LedgerAccountType.Vendor,
            VendorProfileId = payment.VendorProfileId,
            EntryType = LedgerEntryType.AdvancePayout,
            Direction = LedgerDirection.Debit,
            Amount = amount,
            PaymentId = payment.Id,
            BookingId = booking.Id
        }, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "VendorAdvanceIssued",
            EntityType = nameof(Payment),
            EntityId = payment.Id,
            MetadataJson = $"{{\"amount\":{amount}}}"
        }, ct);

        return new VendorAdvanceDto
        {
            PaymentId = payment.Id,
            BookingId = booking.Id,
            VendorProfileId = payment.VendorProfileId,
            Amount = amount,
            IssuedAt = DateTime.UtcNow
        };
    }
}
