using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Availability;
using Ube.Application.Features.Availability.Strategies;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Fraud;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Bookings;
using Ube.Domain.Enums.Listings;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Bookings;

public class CheckoutService : ICheckoutService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IListingUnitRepository _unitRepo;
    private readonly IListingOptionRepository _optionRepo;
    private readonly IBlockedDateRepository _blockedDateRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ISeasonalPricingRepository _seasonalPricingRepo;
    private readonly IListingOfferRepository _offerRepo;
    private readonly StrategySelector _strategySelector;
    private readonly IPaymentService _paymentService;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutService(
        IBookingRepository bookingRepo,
        IListingRepository listingRepo,
        IListingUnitRepository unitRepo,
        IListingOptionRepository optionRepo,
        IBlockedDateRepository blockedDateRepo,
        ICategoryRepository categoryRepo,
        ISeasonalPricingRepository seasonalPricingRepo,
        IListingOfferRepository offerRepo,
        StrategySelector strategySelector,
        IPaymentService paymentService,
        IFraudDetectionService fraudDetectionService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _bookingRepo = bookingRepo;
        _listingRepo = listingRepo;
        _unitRepo = unitRepo;
        _optionRepo = optionRepo;
        _blockedDateRepo = blockedDateRepo;
        _categoryRepo = categoryRepo;
        _seasonalPricingRepo = seasonalPricingRepo;
        _offerRepo = offerRepo;
        _strategySelector = strategySelector;
        _paymentService = paymentService;
        _fraudDetectionService = fraudDetectionService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(Guid customerId, CheckoutRequest request, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new BusinessRuleException("Checkout requires at least one item.");

        var bookings = new List<Booking>();
        var payments = new List<PaymentDto>();

        // Batch-fetch once instead of N sequential round trips for an
        // N-item cart. Categories depend on which listings were actually
        // found, so that lookup happens after the listing one; everything
        // below still validates/throws exactly as before, just via a
        // dictionary lookup instead of a fresh query per item.
        var requestedListingIds = request.Items.Select(i => i.ListingId).Distinct().ToList();
        var listingsById = (await _listingRepo.GetByIdsAsync(requestedListingIds, ct)).ToDictionary(l => l.Id);

        var requestedCategoryIds = listingsById.Values.Select(l => l.CategoryId).Distinct().ToList();
        var categoriesById = (await _categoryRepo.GetByIdsAsync(requestedCategoryIds, ct)).ToDictionary(c => c.Id);

        var requestedUnitIds = request.Items.Where(i => i.ListingUnitId.HasValue).Select(i => i.ListingUnitId!.Value).Distinct().ToList();
        var unitsById = (await _unitRepo.GetByIdsAsync(requestedUnitIds, ct)).ToDictionary(u => u.Id);

        var requestedOptionValueIds = request.Items
            .Where(i => i.OptionValueIds is { Count: > 0 })
            .SelectMany(i => i.OptionValueIds!)
            .Distinct()
            .ToList();
        var optionValuesById = (await _optionRepo.GetValuesByIdsAsync(requestedOptionValueIds, ct)).ToDictionary(v => v.Id);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in request.Items)
            {
                if (!listingsById.TryGetValue(item.ListingId, out var listing))
                    throw new NotFoundException($"Listing {item.ListingId} not found");

                if (!listing.IsActive)
                    throw new BusinessRuleException($"{listing.Title} is no longer available");

                if (!categoriesById.TryGetValue(listing.CategoryId, out var category))
                    throw new NotFoundException("Category not found");

                ListingUnit? unit = null;
                if (item.ListingUnitId.HasValue)
                {
                    if (!unitsById.TryGetValue(item.ListingUnitId.Value, out unit))
                        throw new NotFoundException("Selected unit not found");
                    if (unit.ListingId != listing.Id)
                        throw new BusinessRuleException("Selected unit does not belong to this listing");
                }

                var selectedOptionValues = new List<ListingOptionValue>();
                if (item.OptionValueIds is { Count: > 0 })
                {
                    foreach (var valueId in item.OptionValueIds)
                    {
                        if (!optionValuesById.TryGetValue(valueId, out var optionValue))
                            throw new NotFoundException("Selected option value not found");
                        if (optionValue.Group.ListingId != listing.Id)
                            throw new BusinessRuleException("Selected option does not belong to this listing");
                        selectedOptionValues.Add(optionValue);
                    }
                }

                if (selectedOptionValues.Any(v => v.RequiresSeatSelection) &&
                    (unit == null || unit.Kind != ListingUnitKind.Seat))
                    throw new BusinessRuleException("This option requires selecting a specific seat.");

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

                var optionPriceOverride = selectedOptionValues
                    .LastOrDefault(v => v.PriceOverride.HasValue)?.PriceOverride;
                var effectivePrice = (optionPriceOverride ?? unit?.PriceOverride ?? listing.Price)
                    + selectedOptionValues.Sum(v => v.PriceModifier);
                var effectivePricingUnit = listing.PricingUnitOverride ?? category.ServiceModel;
                var isDateBasedPricing = effectivePricingUnit is PricingUnit.PerNight or PricingUnit.PerDay;
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
                        effectivePrice, item.Quantity, item.StartDateTime, item.EndDateTime, effectivePricingUnit);
                }

                // Applied once per booking, on top of any seasonal
                // adjustment - a vendor offer is a marketing deal, not a
                // rate structure, so it never affects the per-night math.
                var activeOffer = await _offerRepo.GetActiveDiscountForListingAsync(
                    listing.Id, BusinessDate.Today, ct);
                totalAmount = BookingPricingRules.ApplyOfferDiscount(totalAmount, activeOffer);

                var effectiveBookingType = selectedOptionValues.FirstOrDefault(v => v.ConfirmationTypeOverride != null)?.ConfirmationTypeOverride
                    ?? category.BookingType;
                var status = effectiveBookingType == BookingConfirmationType.Instant
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
                    SelectedOptionValueIds = selectedOptionValues.Count > 0
                        ? string.Join(",", selectedOptionValues.Select(v => v.Id))
                        : null,
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
        // FK ids set, not the loaded entities. Batched into one round trip
        // instead of one GetByIdAsync per booking.
        var hydratedBookingsById = (await _bookingRepo.GetByIdsAsync(bookings.Select(b => b.Id), ct))
            .ToDictionary(b => b.Id);

        var hydratedBookings = new List<BookingDetailDto>();
        foreach (var booking in bookings)
        {
            if (!hydratedBookingsById.TryGetValue(booking.Id, out var hydrated))
                throw new NotFoundException("Booking not found after creation");
            hydratedBookings.Add(MapToDetail(hydrated));

            // Best-effort - a notification failure should never fail a
            // checkout that has already been paid for and committed.
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = hydrated.Listing.VendorProfile.UserId,
                    Title = "New booking request",
                    Message = $"{hydrated.Customer.FirstName} {hydrated.Customer.LastName} booked {hydrated.Listing.Title} ({hydrated.BookingNumber}).",
                    Type = (int)NotificationType.NewBookingRequest
                }, ct);
            }
            catch
            {
                // Swallow - notification delivery is not part of the checkout contract.
            }
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
