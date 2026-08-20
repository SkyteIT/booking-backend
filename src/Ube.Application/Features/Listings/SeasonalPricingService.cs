using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class SeasonalPricingService : ISeasonalPricingService
{
    private readonly ISeasonalPricingRepository _ruleRepo;
    private readonly IListingOfferRepository _offerRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IListingUnitRepository _unitRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IVendorProfileRepository _vendorProfileRepo;

    public SeasonalPricingService(
        ISeasonalPricingRepository ruleRepo,
        IListingOfferRepository offerRepo,
        IListingRepository listingRepo,
        IListingUnitRepository unitRepo,
        ICategoryRepository categoryRepo,
        IVendorProfileRepository vendorProfileRepo)
    {
        _ruleRepo = ruleRepo;
        _offerRepo = offerRepo;
        _listingRepo = listingRepo;
        _unitRepo = unitRepo;
        _categoryRepo = categoryRepo;
        _vendorProfileRepo = vendorProfileRepo;
    }

    public async Task<IReadOnlyList<SeasonalPricingRuleDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var rules = await _ruleRepo.GetByListingIdAsync(listingId, ct);
        return rules.Select(ToDto).ToList();
    }

    public async Task<SeasonalPricingRuleDto> CreateAsync(Guid listingId, Guid userId, CreateSeasonalPricingRuleRequest request, CancellationToken ct = default)
    {
        var listing = await EnsureOwnedListingAsync(listingId, userId, ct);
        await EnsureSeasonalPricingSupportedAsync(listing.CategoryId, ct);

        if (request.ListingUnitId.HasValue)
        {
            var unit = await _unitRepo.GetByIdAsync(request.ListingUnitId.Value, ct)
                ?? throw new NotFoundException("Listing unit not found");
            if (unit.ListingId != listingId)
                throw new BusinessRuleException("Selected unit does not belong to this listing");
        }

        ValidateDateRange(request.StartDate, request.EndDate);

        var overlaps = await _ruleRepo.HasOverlapAsync(listingId, request.ListingUnitId, request.StartDate, request.EndDate, excludeRuleId: null, ct);
        if (overlaps)
            throw new BusinessRuleException("This date range overlaps an existing seasonal rate rule for this listing.");

        var rule = new SeasonalPricingRule
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            ListingUnitId = request.ListingUnitId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AdjustmentType = request.AdjustmentType,
            AdjustmentValue = request.AdjustmentValue
        };

        await _ruleRepo.AddAsync(rule, ct);
        return ToDto(rule);
    }

    public async Task<SeasonalPricingRuleDto> UpdateAsync(Guid listingId, Guid ruleId, Guid userId, UpdateSeasonalPricingRuleRequest request, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var rule = await _ruleRepo.GetByIdAsync(ruleId, ct)
            ?? throw new NotFoundException("Seasonal pricing rule not found");
        if (rule.ListingId != listingId)
            throw new NotFoundException("Seasonal pricing rule not found");

        var newStart = request.StartDate ?? rule.StartDate;
        var newEnd = request.EndDate ?? rule.EndDate;
        ValidateDateRange(newStart, newEnd);

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var overlaps = await _ruleRepo.HasOverlapAsync(listingId, rule.ListingUnitId, newStart, newEnd, excludeRuleId: rule.Id, ct);
            if (overlaps)
                throw new BusinessRuleException("This date range overlaps an existing seasonal rate rule for this listing.");
        }

        if (request.Name is not null) rule.Name = request.Name;
        rule.StartDate = newStart;
        rule.EndDate = newEnd;
        if (request.AdjustmentType.HasValue) rule.AdjustmentType = request.AdjustmentType.Value;
        if (request.AdjustmentValue.HasValue) rule.AdjustmentValue = request.AdjustmentValue.Value;
        if (request.IsActive.HasValue) rule.IsActive = request.IsActive.Value;
        rule.UpdatedAt = DateTime.UtcNow;

        await _ruleRepo.UpdateAsync(rule, ct);
        return ToDto(rule);
    }

    public async Task DeleteAsync(Guid listingId, Guid ruleId, Guid userId, CancellationToken ct = default)
    {
        await EnsureOwnedListingAsync(listingId, userId, ct);

        var rule = await _ruleRepo.GetByIdAsync(ruleId, ct)
            ?? throw new NotFoundException("Seasonal pricing rule not found");
        if (rule.ListingId != listingId)
            throw new NotFoundException("Seasonal pricing rule not found");

        await _ruleRepo.DeleteAsync(rule, ct);
    }

    public async Task<PriceQuoteDto> GetPriceQuoteAsync(Guid listingId, Guid? listingUnitId, DateTime startDate, DateTime endDate, int quantity, CancellationToken ct = default)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var category = await _categoryRepo.GetByIdAsync(listing.CategoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        decimal effectivePrice = listing.Price;
        if (listingUnitId.HasValue)
        {
            var unit = await _unitRepo.GetByIdAsync(listingUnitId.Value, ct)
                ?? throw new NotFoundException("Selected unit not found");
            effectivePrice = unit.PriceOverride ?? listing.Price;
        }

        var isDateBased = category.ServiceModel is PricingUnit.PerNight or PricingUnit.PerDay;
        decimal total;
        if (!isDateBased)
        {
            total = BookingPricingRules.CalculateTotal(effectivePrice, quantity, startDate, endDate, category.ServiceModel);
        }
        else
        {
            var rules = await _ruleRepo.GetActiveInRangeAsync(
                listingId, listingUnitId, DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate), ct);
            total = BookingPricingRules.CalculateSeasonalTotal(effectivePrice, quantity, startDate, endDate, rules);
        }

        // Same lookup/apply CheckoutService uses - the quote can never
        // drift from the real charge.
        var activeOffer = await _offerRepo.GetActiveDiscountForListingAsync(listingId, BusinessDate.Today, ct);
        total = BookingPricingRules.ApplyOfferDiscount(total, activeOffer);

        return new PriceQuoteDto { TotalAmount = total, Currency = listing.Currency };
    }

    private static void ValidateDateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new BusinessRuleException("End date must be on or after the start date.");
    }

    // Seasonal rules only make sense for date-based pricing (PerNight/
    // PerDay) - other pricing units have no per-date concept to attach
    // a season to (BookingPricingRules.CalculateTotal treats them flat).
    private async Task EnsureSeasonalPricingSupportedAsync(Guid categoryId, CancellationToken ct)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct: ct)
            ?? throw new NotFoundException("Category not found");

        if (category.ServiceModel is not (PricingUnit.PerNight or PricingUnit.PerDay))
            throw new BusinessRuleException("Seasonal pricing only applies to per-night or per-day listings.");
    }

    private async Task<Listing> EnsureOwnedListingAsync(Guid listingId, Guid userId, CancellationToken ct)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var vendorProfile = await _vendorProfileRepo.GetVendorIdAsync(userId);
        if (vendorProfile == null || vendorProfile.Id != listing.VendorProfileId)
            throw new ForbiddenException("You do not have permission to manage pricing for this listing.");

        return listing;
    }

    private static SeasonalPricingRuleDto ToDto(SeasonalPricingRule r) => new()
    {
        Id = r.Id,
        ListingId = r.ListingId,
        ListingUnitId = r.ListingUnitId,
        Name = r.Name,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        AdjustmentType = r.AdjustmentType,
        AdjustmentValue = r.AdjustmentValue,
        IsActive = r.IsActive
    };
}
