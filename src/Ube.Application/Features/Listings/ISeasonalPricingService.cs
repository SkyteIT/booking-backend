namespace Ube.Application.Features.Listings;

public interface ISeasonalPricingService
{
    Task<IReadOnlyList<SeasonalPricingRuleDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<SeasonalPricingRuleDto> CreateAsync(Guid listingId, Guid userId, CreateSeasonalPricingRuleRequest request, CancellationToken ct = default);
    Task<SeasonalPricingRuleDto> UpdateAsync(Guid listingId, Guid ruleId, Guid userId, UpdateSeasonalPricingRuleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid listingId, Guid ruleId, Guid userId, CancellationToken ct = default);

    // Public - reuses the same seasonal-aware calculation checkout uses,
    // so a customer's preview always matches the real charge.
    Task<PriceQuoteDto> GetPriceQuoteAsync(Guid listingId, Guid? listingUnitId, DateTime startDate, DateTime endDate, int quantity, CancellationToken ct = default);
}
