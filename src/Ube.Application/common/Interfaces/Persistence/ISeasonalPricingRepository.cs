using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface ISeasonalPricingRepository
{
    Task<SeasonalPricingRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<SeasonalPricingRule>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);

    // Active rules for this listing (or specifically this unit, when
   
    Task<List<SeasonalPricingRule>> GetActiveInRangeAsync(Guid listingId, Guid? listingUnitId, DateOnly start, DateOnly end, CancellationToken ct = default);

    // True if any OTHER active rule for the same listing/unit overlaps this range - used to reject ambiguous overlapping rules on write.
    Task<bool> HasOverlapAsync(Guid listingId, Guid? listingUnitId, DateOnly start, DateOnly end, Guid? excludeRuleId, CancellationToken ct = default);

    Task AddAsync(SeasonalPricingRule rule, CancellationToken ct = default);
    Task UpdateAsync(SeasonalPricingRule rule, CancellationToken ct = default);
    Task DeleteAsync(SeasonalPricingRule rule, CancellationToken ct = default);
}
