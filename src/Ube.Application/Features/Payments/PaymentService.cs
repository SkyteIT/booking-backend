using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Content.Category;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ICommissionResolverService _commissionResolver;
    private readonly IPaymentGatewayClient _gateway;

    public PaymentService(
        IPaymentRepository paymentRepo,
        ILedgerRepository ledgerRepo,
        IBookingRepository bookingRepo,
        ICategoryRepository categoryRepo,
        ICommissionResolverService commissionResolver,
        IPaymentGatewayClient gateway)
    {
        _paymentRepo = paymentRepo;
        _ledgerRepo = ledgerRepo;
        _bookingRepo = bookingRepo;
        _categoryRepo = categoryRepo;
        _commissionResolver = commissionResolver;
        _gateway = gateway;
    }

    public async Task<PaymentDto> InitiateAsync(Guid initiatedByUserId, InitiatePaymentRequest request, CancellationToken ct = default)
    {
        // Idempotency: a repeat request with the same key returns the
        // existing Payment instead of creating a new one.
        var existing = await _paymentRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing != null)
            return ToDto(existing);

        var booking = await _bookingRepo.GetByIdAsync(request.BookingId)
            ?? throw new NotFoundException("Booking not found");

        if (booking.CustomerId != initiatedByUserId)
            throw new ForbiddenException("You can only pay for your own booking");

        var vendorProfileId = booking.Listing.VendorProfileId;
        var categoryId = booking.Listing.CategoryId;

        var category = await _categoryRepo.GetByIdAsync(categoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        var resolution = await _commissionResolver.ResolveAsync(vendorProfileId, categoryId, DateTime.UtcNow, ct);
        var split = MoneyMath.SplitCommission(booking.TotalAmount, resolution.CommissionPercent, category.PlatformServiceFee);

        var chargeResult = await _gateway.InitiateChargeAsync(
            booking.TotalAmount, booking.Currency, request.IdempotencyKey, booking.Id.ToString(), ct);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            VendorProfileId = vendorProfileId,
            InitiatedByUserId = initiatedByUserId,
            Amount = booking.TotalAmount,
            Currency = booking.Currency,
            CollectionMethod = request.CollectionMethod,
            Status = MapStatus(chargeResult.Status),
            GatewayReference = chargeResult.ExternalReference,
            IdempotencyKey = request.IdempotencyKey,
            CommissionPercentApplied = resolution.CommissionPercent,
            CommissionAmount = split.CommissionAmount,
            PlatformFeeAmount = split.PlatformFeeAmount,
            NetVendorAmount = split.NetVendorAmount
        };

        await _paymentRepo.AddAsync(payment, ct);

        if (payment.Status == PaymentStatus.Captured)
            await WriteChargeLedgerEntriesAsync(payment, ct);

        return ToDto(payment);
    }

    public async Task<PaymentDto> GetAsync(Guid paymentId, Guid requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException("Payment not found");

        if (!isAdmin)
        {
            var booking = await _bookingRepo.GetByIdAsync(payment.BookingId);
            var isOwner = booking != null && booking.CustomerId == requestingUserId;
            var isVendor = booking != null && booking.Listing.VendorProfile.UserId == requestingUserId;
            if (!isOwner && !isVendor)
                throw new ForbiddenException("You do not have access to this payment");
        }

        return ToDto(payment);
    }

    private async Task WriteChargeLedgerEntriesAsync(Payment payment, CancellationToken ct)
    {
        var entries = new List<LedgerEntry>();

        if (payment.CollectionMethod == PaymentCollectionMethod.PlatformCollected)
        {
            // Platform received the full amount, and now owes the vendor
            // their net cut - a balanced Debit/Credit pair.
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Charge,
                Direction = LedgerDirection.Credit,
                Amount = payment.Amount,
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Commission,
                Direction = LedgerDirection.Debit,
                Amount = payment.NetVendorAmount,
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = LedgerEntryType.Commission,
                Direction = LedgerDirection.Credit,
                Amount = payment.NetVendorAmount,
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            });
        }
        else
        {
            // Vendor collected the cash directly - platform never held it,
            // but the vendor now owes the platform its commission.
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Vendor,
                VendorProfileId = payment.VendorProfileId,
                EntryType = LedgerEntryType.Commission,
                Direction = LedgerDirection.Debit,
                Amount = payment.CommissionAmount,
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            });
            entries.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountType = LedgerAccountType.Platform,
                EntryType = LedgerEntryType.Commission,
                Direction = LedgerDirection.Credit,
                Amount = payment.CommissionAmount,
                BookingId = payment.BookingId,
                PaymentId = payment.Id
            });
        }

        await _ledgerRepo.AddRangeAsync(entries, ct);
    }

    private static PaymentStatus MapStatus(GatewayChargeStatus status) => status switch
    {
        GatewayChargeStatus.Succeeded => PaymentStatus.Captured,
        GatewayChargeStatus.Failed => PaymentStatus.Failed,
        _ => PaymentStatus.Pending
    };

    private static PaymentDto ToDto(Payment p) => new()
    {
        Id = p.Id,
        BookingId = p.BookingId,
        VendorProfileId = p.VendorProfileId,
        Amount = p.Amount,
        Currency = p.Currency,
        CollectionMethod = p.CollectionMethod,
        Status = p.Status,
        GatewayReference = p.GatewayReference,
        CommissionPercentApplied = p.CommissionPercentApplied,
        CommissionAmount = p.CommissionAmount,
        PlatformFeeAmount = p.PlatformFeeAmount,
        NetVendorAmount = p.NetVendorAmount,
        CreatedAt = p.CreatedAt
    };
}
