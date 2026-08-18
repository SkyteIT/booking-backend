using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Fraud;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Bookings;

public class CheckoutService : ICheckoutService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IListingUnitRepository _unitRepo;
    private readonly IBlockedDateRepository _blockedDateRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ISeasonalPricingRepository _seasonalPricingRepo;
    private readonly StrategySelector _strategySelector;
    private readonly IPaymentService _paymentService;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(
        IBookingRepository bookingRepo,
        IListingRepository listingRepo,
        IListingUnitRepository unitRepo,
        IBlockedDateRepository blockedDateRepo,
        ICategoryRepository categoryRepo,
        ISeasonalPricingRepository seasonalPricingRepo,
        StrategySelector strategySelector,
        IPaymentService paymentService,
        IFraudDetectionService fraudDetectionService,
        IUnitOfWork unitOfWork)
    {
        _bookingRepo = bookingRepo;
        _listingRepo = listingRepo;
        _unitRepo = unitRepo;
        _blockedDateRepo = blockedDateRepo;
        _categoryRepo = categoryRepo;
        _seasonalPricingRepo = seasonalPricingRepo;
        _strategySelector = strategySelector;
        _paymentService = paymentService;
        _fraudDetectionService = fraudDetectionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(Guid customerId, CheckoutRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new BusinessRuleException("Checkout requires at least one item.");

        var bookings = new List<Booking>();
        var payments = new List<PaymentDto>();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in request.Items)
            {
                var listing = await _listingRepo.GetByIdAsync(item.ListingId)
                    ?? throw new NotFoundException($"Listing {item.ListingId} not found");

                if (!listing.IsActive)
                    throw new BusinessRuleException($"{listing.Title} is no longer available");

                var category = await _categoryRepo.GetByIdAsync(listing.CategoryId, ct: ct)
                    ?? throw new NotFoundException("Category not found");

                ListingUnit? unit = null;
                if (item.ListingUnitId.HasValue)
                {
                    unit = await _unitRepo.GetByIdAsync(item.ListingUnitId.Value, ct)
                        ?? throw new NotFoundException("Selected unit not found");
                    if (unit.ListingId != listing.Id)
                        throw new BusinessRuleException("Selected unit does not belong to this listing");
                }

                // Duplicate-booking guard - same customer already holds a
                // live (Pending/Confirmed) booking for this listing/unit that
                // overlaps these dates. Unambiguous fraud signal, always
                // hard-declined, never just flagged.
                var isDuplicate = await _bookingRepo.HasOverlappingBookingForCustomerAsync(
                    customerId, listing.Id, item.ListingUnitId, item.StartDateTime, item.EndDateTime, ct);
                if (isDuplicate)
                    throw new BusinessRuleException($"You already have a booking for {listing.Title} in this date range.");

                await EnsureAvailableAsync(listing, unit, item.StartDateTime, item.EndDateTime, ct);

                var nextValue = await _bookingRepo.GetNextBookingSequenceAsync();
                var bookingNumber = $"BKG-{nextValue:D6}";

                var effectivePrice = unit?.PriceOverride ?? listing.Price;
                var isDateBasedPricing = category.ServiceModel is PricingUnit.PerNight or PricingUnit.PerDay;
                decimal totalAmount;
                if (isDateBasedPricing)
                {
                    var seasonalRules = await _seasonalPricingRepo.GetActiveInRangeAsync(
                        listing.Id, item.ListingUnitId,
                        DateOnly.FromDateTime(item.StartDateTime), DateOnly.FromDateTime(item.EndDateTime), ct);
                    totalAmount = BookingPricingRules.CalculateSeasonalTotal(
                        effectivePrice, item.Quantity, item.StartDateTime, item.EndDateTime, seasonalRules);
                }
                else
                {
                    totalAmount = BookingPricingRules.CalculateTotal(
                        effectivePrice, item.Quantity, item.StartDateTime, item.EndDateTime, category.ServiceModel);
                }

                var status = category.BookingType == BookingConfirmationType.Instant
                    ? BookingStatus.Confirmed
                    : BookingStatus.Pending;

                var collectionMethod = category.PaymentCollectionModel == ServiceCollectionModel.PayAtVenue
                    ? PaymentCollectionMethod.VendorCollected
                    : PaymentCollectionMethod.PlatformCollected;

                // Gray-area fraud signal: a brand-new account making an
                // unusually large first booking. Held for admin review
                // instead of declined outright - the booking is created but
                // stays Pending and payment capture is deferred.
                var isHeldForFraudReview = await _fraudDetectionService.IsNewAccountHighValueAsync(customerId, totalAmount, ct);

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    BookingNumber = bookingNumber,
                    ListingId = listing.Id,
                    ListingUnitId = item.ListingUnitId,
                    CustomerId = customerId,
                    StartDateTime = item.StartDateTime,
                    EndDateTime = item.EndDateTime,
                    Status = isHeldForFraudReview ? BookingStatus.Pending : status,
                    IsHeldForFraudReview = isHeldForFraudReview,
                    TotalAmount = totalAmount,
                    Currency = listing.Currency,
                    CreatedAt = DateTime.UtcNow
                };

                await _bookingRepo.AddAsync(booking, ct);
                bookings.Add(booking);

                if (isHeldForFraudReview)
                {
                    // No payment initiated while held - deferred until an
                    // admin clears the flag (Features/Fraud/FraudDetectionService.ReviewAsync).
                    await _fraudDetectionService.CreateHoldFlagAsync(booking, status, collectionMethod, ct);
                }
                else
                {
                    var paymentRequest = new InitiatePaymentRequest
                    {
                        BookingId = booking.Id,
                        CollectionMethod = collectionMethod,
                        IdempotencyKey = $"{request.IdempotencyKey}:{booking.Id}"
                    };
                    var payment = await _paymentService.InitiateAsync(customerId, paymentRequest, ct);
                    payments.Add(payment);

                    if (payment.Status == PaymentStatus.Failed)
                        throw new BusinessRuleException($"Payment failed for {listing.Title}");
                }
            }

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // Post-commit, weaker pattern signals (velocity, repeated
        // cancellations) - the checkout already succeeded, this only ever
        // adds visibility for admins, never affects the booking itself.
        foreach (var booking in bookings)
        {
            await _fraudDetectionService.EvaluateFlagOnlyRulesAsync(customerId, booking.Id, ct);
        }

        // Re-fetch with navigation properties populated (Listing/Customer)
        // for the response DTO - the Booking objects built above only have
        // FK ids set, not the loaded entities.
        var hydratedBookings = new List<BookingDetailDto>();
        foreach (var booking in bookings)
        {
            var hydrated = await _bookingRepo.GetByIdAsync(booking.Id)
                ?? throw new NotFoundException("Booking not found after creation");
            hydratedBookings.Add(MapToDetail(hydrated));
        }

        return new CheckoutResultDto
        {
            Bookings = hydratedBookings,
            Payments = payments
        };
    }

    private async Task EnsureAvailableAsync(Listing listing, ListingUnit? unit, DateTime start, DateTime end, CancellationToken ct)
    {
        var strategy = _strategySelector.GetStrategy(listing.AvailabilityType);
        var capacity = unit?.Capacity ?? listing.Capacity;

        var existingBookings = unit != null
            ? await _bookingRepo.GetBookingsByListingUnitAndDateRangeAsync(unit.Id, start, end, ct)
            : await _bookingRepo.GetBookingsByListingAndDateRangeAsync(listing.Id, start, end);

        var blockedDates = await _blockedDateRepo.GetByListingAndDateRangeAsync(listing.Id, start, end);
        var blockedDateSet = blockedDates.Select(b => b.Date.Date).ToHashSet();

        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var bookedCount = existingBookings.Count(b => b.StartDateTime.Date <= date && b.EndDateTime.Date >= date);
            var isBlocked = blockedDateSet.Contains(date);

            var result = strategy.CalculateAvailability(date, capacity, bookedCount, isBlocked);
            if (result.Status != AvailabilityStatus.Available)
                throw new BusinessRuleException($"{listing.Title} is not available on {date:yyyy-MM-dd}");
        }
    }

    private static BookingDetailDto MapToDetail(Booking booking) => new()
    {
        BookingId = booking.Id,
        BookingNumber = booking.BookingNumber,
        ListingTitle = booking.Listing.Title,
        CustomerName = booking.Customer.FirstName + " " + booking.Customer.LastName,
        CustomerEmail = booking.Customer.Email,
        StartDateTime = booking.StartDateTime,
        EndDateTime = booking.EndDateTime,
        Status = booking.Status,
        TotalAmount = booking.TotalAmount,
        Currency = booking.Currency,
        CreatedAt = booking.CreatedAt,
        CanConfirm = false,
        CanReject = false,
        CanCancel = false
    };
}
