using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Category;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class RefundService : IRefundService
{
    private readonly IRefundRepository _refundRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IPaymentAuditLogRepository _auditRepo;
    private readonly IPaymentGatewayClient _gateway;

    public RefundService(
        IRefundRepository refundRepo,
        IPaymentRepository paymentRepo,
        ILedgerRepository ledgerRepo,
        IBookingRepository bookingRepo,
        ICategoryRepository categoryRepo,
        IPaymentAuditLogRepository auditRepo,
        IPaymentGatewayClient gateway)
    {
        _refundRepo = refundRepo;
        _paymentRepo = paymentRepo;
        _ledgerRepo = ledgerRepo;
        _bookingRepo = bookingRepo;
        _categoryRepo = categoryRepo;
        _auditRepo = auditRepo;
        _gateway = gateway;
    }

    public async Task<RefundDto> RequestAsync(Guid requestedByUserId, bool isAdmin, RequestRefundRequest request, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(request.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        var booking = await _bookingRepo.GetByIdAsync(payment.BookingId)
            ?? throw new NotFoundException("Booking not found");

        if (!isAdmin && booking.CustomerId != requestedByUserId)
            throw new ForbiddenException("You can only request a refund for your own booking");

        if (payment.Status != Domain.Enums.Payments.PaymentStatus.Captured)
            throw new BusinessRuleException("Only captured payments can be refunded");

        var category = await _categoryRepo.GetByIdAsync(booking.Listing.CategoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        var daysBeforeStart = (booking.StartDateTime - DateTime.UtcNow).TotalDays;
        decimal policyPercent;
        if (daysBeforeStart >= category.FullRefundDaysBefore)
            policyPercent = 100m;
        else if (daysBeforeStart >= category.PartialRefundDaysBefore)
            policyPercent = category.PartialRefundPercent;
        else
            policyPercent = 0m;

        var refundAmount = MoneyMath.PercentOf(payment.Amount, policyPercent);

        var refund = new Refund
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Amount = refundAmount,
            Reason = request.Reason,
            PolicyTierApplied = policyPercent,
            RequestedByUserId = requestedByUserId,
            Status = RefundStatus.Requested
        };

        // Maker-checker: small in-policy refunds under the configured
        // threshold auto-approve; anything above it needs an explicit,
        // separate admin approval action.
        var autoApprove = category.RefundAutoApprovalThreshold.HasValue
            && refundAmount <= category.RefundAutoApprovalThreshold.Value;

        await _refundRepo.AddAsync(refund, ct);

        if (autoApprove)
            await ProcessApprovedRefundAsync(refund, payment, approvedByUserId: null, ct);

        return ToDto(refund);
    }

    public async Task<RefundDto> ApproveAsync(Guid approvedByUserId, Guid refundId, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdAsync(refundId, ct)
            ?? throw new NotFoundException("Refund not found");

        if (refund.Status != RefundStatus.Requested)
            throw new BusinessRuleException("Only requested refunds can be approved");

        var payment = await _paymentRepo.GetByIdAsync(refund.PaymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        refund.Status = RefundStatus.Approved;
        refund.ApprovedByUserId = approvedByUserId;
        await _refundRepo.UpdateAsync(refund, ct);

        await ProcessApprovedRefundAsync(refund, payment, approvedByUserId, ct);

        return ToDto(refund);
    }

    public async Task<RefundDto> RejectAsync(Guid actorUserId, Guid refundId, string reason, CancellationToken ct = default)
    {
        var refund = await _refundRepo.GetByIdAsync(refundId, ct)
            ?? throw new NotFoundException("Refund not found");

        if (refund.Status != RefundStatus.Requested)
            throw new BusinessRuleException("Only requested refunds can be rejected");

        refund.Status = RefundStatus.Rejected;
        await _refundRepo.UpdateAsync(refund, ct);

        // The ledger never sees a rejected refund - it's the audit log's job
        // to capture this decision.
        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "RefundRejected",
            EntityType = nameof(Refund),
            EntityId = refund.Id,
            MetadataJson = $"{{\"reason\":\"{reason}\"}}"
        }, ct);

        return ToDto(refund);
    }

    private async Task ProcessApprovedRefundAsync(Refund refund, Payment payment, Guid? approvedByUserId, CancellationToken ct)
    {
        await _gateway.InitiateRefundAsync(payment.GatewayReference ?? string.Empty, refund.Amount, refund.Id.ToString(), ct);

        refund.Status = RefundStatus.Processed;
        refund.ProcessedAt = DateTime.UtcNow;
        await _refundRepo.UpdateAsync(refund, ct);

        payment.Status = refund.Amount >= payment.Amount
            ? Domain.Enums.Payments.PaymentStatus.Refunded
            : Domain.Enums.Payments.PaymentStatus.PartiallyRefunded;
        await _paymentRepo.UpdateAsync(payment, ct);

        // Reversing entries - never mutates the original charge entries.
        var refundShareOfCommission = payment.Amount == 0
            ? 0
            : MoneyMath.RoundCurrency(payment.NetVendorAmount * refund.Amount / payment.Amount);

        var entries = new List<LedgerEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Refund,
                Direction = LedgerDirection.Debit,
                Amount = refund.Amount,
                PaymentId = payment.Id,
                RefundId = refund.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = LedgerEntryType.Refund,
                Direction = LedgerDirection.Debit,
                Amount = refundShareOfCommission,
                PaymentId = payment.Id,
                RefundId = refund.Id
            }
        };

        await _ledgerRepo.AddRangeAsync(entries, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = approvedByUserId ?? refund.RequestedByUserId,
            Action = approvedByUserId.HasValue ? "RefundApproved" : "RefundAutoApproved",
            EntityType = nameof(Refund),
            EntityId = refund.Id
        }, ct);
    }

    private static RefundDto ToDto(Refund r) => new()
    {
        Id = r.Id,
        PaymentId = r.PaymentId,
        Amount = r.Amount,
        Reason = r.Reason,
        Status = r.Status,
        PolicyTierApplied = r.PolicyTierApplied,
        ProcessedAt = r.ProcessedAt,
        CreatedAt = r.CreatedAt
    };
}
