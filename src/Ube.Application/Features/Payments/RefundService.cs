using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Payments;
using Ube.Domain.Enums.Users;

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
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly INotificationService _notificationService;
    private readonly IAdminAlertService _adminAlertService;

    public RefundService(
        IRefundRepository refundRepo,
        IPaymentRepository paymentRepo,
        ILedgerRepository ledgerRepo,
        IBookingRepository bookingRepo,
        ICategoryRepository categoryRepo,
        IPaymentAuditLogRepository auditRepo,
        IPaymentGatewayClient gateway,
        IVendorProfileRepository vendorRepo,
        INotificationService notificationService,
        IAdminAlertService adminAlertService)
    {
        _refundRepo = refundRepo;
        _paymentRepo = paymentRepo;
        _ledgerRepo = ledgerRepo;
        _bookingRepo = bookingRepo;
        _categoryRepo = categoryRepo;
        _auditRepo = auditRepo;
        _gateway = gateway;
        _vendorRepo = vendorRepo;
        _notificationService = notificationService;
        _adminAlertService = adminAlertService;
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
        {
            await ProcessApprovedRefundAsync(refund, payment, approvedByUserId: null, ct);
        }
        else
        {
            await _adminAlertService.NotifyRolesAsync(
                new[] { UserRole.Finance, UserRole.SuperAdmin },
                "Refund needs approval",
                $"A refund of {refundAmount:F2} {payment.Currency} is pending your approval: {request.Reason}.",
                NotificationType.RefundPending,
                ct);
        }

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

    public async Task<PagedResult<AdminRefundDto>> GetPagedAsync(RefundStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _refundRepo.GetPagedAsync(status, pageNumber, pageSize, ct);

        var mapped = items.Select(x => new AdminRefundDto
        {
            Id = x.Refund.Id,
            PaymentId = x.Refund.PaymentId,
            BookingNumber = x.BookingNumber,
            CustomerName = x.CustomerName,
            VendorName = x.VendorName,
            ListingTitle = x.ListingTitle,
            Amount = x.Refund.Amount,
            Reason = x.Refund.Reason,
            Status = x.Refund.Status,
            PolicyTierApplied = x.Refund.PolicyTierApplied,
            ProcessedAt = x.Refund.ProcessedAt,
            CreatedAt = x.Refund.CreatedAt
        }).ToList();

        return new PagedResult<AdminRefundDto>
        {
            Items = mapped,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    private async Task ProcessApprovedRefundAsync(Refund refund, Payment payment, Guid? approvedByUserId, CancellationToken ct)
    {
        // Only a PlatformCollected payment ever moved funds through the
        // gateway - for VendorCollected the vendor took the customer's cash
        // directly, so there's nothing captured on the platform's side to
        // refund through it (refunding the customer there is the vendor's
        // problem; the platform's only exposure was the commission it was
        // owed, reversed in the ledger entries below).
        if (payment.CollectionMethod == PaymentCollectionMethod.PlatformCollected)
        {
            await _gateway.InitiateRefundAsync(payment.GatewayReference ?? string.Empty, refund.Amount, refund.Id.ToString(), ct);
        }

        refund.Status = RefundStatus.Processed;
        refund.ProcessedAt = DateTime.UtcNow;
        await _refundRepo.UpdateAsync(refund, ct);

        payment.Status = refund.Amount >= payment.Amount
            ? Domain.Enums.Payments.PaymentStatus.Refunded
            : Domain.Enums.Payments.PaymentStatus.PartiallyRefunded;
        await _paymentRepo.UpdateAsync(payment, ct);

        // If the vendor already received an advance against this payment,
        // this debit is money they need to give back, not just a smaller
        // future payout - tag it Clawback instead of Refund so reporting
        // can tell the two apart. The ledger math is identical either way;
        // this only changes the entry's label.
        var existingEntries = await _ledgerRepo.GetByPaymentIdAsync(payment.Id, ct);
        var isClawback = existingEntries.Any(e => e.EntryType == LedgerEntryType.AdvancePayout);
        var vendorEntryType = isClawback ? LedgerEntryType.Clawback : LedgerEntryType.Refund;

        // Reversing entries - never mutates the original charge entries.
        // Mirrors WriteChargeLedgerEntriesAsync's own CollectionMethod
        // branch exactly, so a refund only ever reverses what the charge
        // actually moved, and every debit still has its offsetting credit
        // the way the original charge entries did.
        var entries = new List<LedgerEntry>();

        if (payment.CollectionMethod == PaymentCollectionMethod.PlatformCollected)
        {
            var vendorShare = payment.Amount == 0
                ? 0
                : MoneyMath.RoundCurrency(payment.NetVendorAmount * refund.Amount / payment.Amount);

            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Refund,
                Direction = LedgerDirection.Debit,
                Amount = refund.Amount,
                PaymentId = payment.Id,
                RefundId = refund.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = vendorEntryType,
                Direction = LedgerDirection.Credit,
                Amount = vendorShare,
                PaymentId = payment.Id,
                RefundId = refund.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = vendorEntryType,
                Direction = LedgerDirection.Debit,
                Amount = vendorShare,
                PaymentId = payment.Id,
                RefundId = refund.Id
            });
        }
        else
        {
            // The platform's only stake in a VendorCollected payment was
            // the commission the vendor owed on top of it - refunding the
            // booking only reverses that commission claim, not a gross
            // amount the platform never held.
            var commissionShare = payment.Amount == 0
                ? 0
                : MoneyMath.RoundCurrency(payment.CommissionAmount * refund.Amount / payment.Amount);

            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = vendorEntryType,
                Direction = LedgerDirection.Credit,
                Amount = commissionShare,
                PaymentId = payment.Id,
                RefundId = refund.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = vendorEntryType,
                Direction = LedgerDirection.Debit,
                Amount = commissionShare,
                PaymentId = payment.Id,
                RefundId = refund.Id
            });
        }

        await _ledgerRepo.AddRangeAsync(entries, ct);

        await _auditRepo.AddAsync(new PaymentAuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = approvedByUserId ?? refund.RequestedByUserId,
            Action = approvedByUserId.HasValue ? "RefundApproved" : "RefundAutoApproved",
            EntityType = nameof(Refund),
            EntityId = refund.Id
        }, ct);

        var vendor = await _vendorRepo.GetByIdAsync(payment.VendorProfileId);
        if (vendor != null)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = vendor.UserId,
                    Title = "Refund processed",
                    Message = $"A refund of {refund.Amount:F2} {payment.Currency} was processed for {refund.Reason}.",
                    Type = (int)NotificationType.RefundProcessed
                }, ct);
            }
            catch
            {
                // Best-effort - a notification failure never blocks a refund that's already gone through the gateway.
            }
        }
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
