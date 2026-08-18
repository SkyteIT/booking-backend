using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class SeasonalPricingRepository : ISeasonalPricingRepository
{
    private readonly ApplicationDbContext _db;

    public SeasonalPricingRepository(ApplicationDbContext db) => _db = db;

    public async Task<SeasonalPricingRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.SeasonalPricingRules.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<SeasonalPricingRule>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default)
        => await _db.SeasonalPricingRules
            .Where(x => x.ListingId == listingId)
            .OrderBy(x => x.StartDate)
            .ToListAsync(ct);

    public async Task<List<SeasonalPricingRule>> GetActiveInRangeAsync(
        Guid listingId, Guid? listingUnitId, DateOnly start, DateOnly end, CancellationToken ct = default)
        => await _db.SeasonalPricingRules
            .Where(x => x.ListingId == listingId
                && x.ListingUnitId == listingUnitId
                && x.IsActive
                && x.StartDate <= end
                && x.EndDate >= start)
            .ToListAsync(ct);

    public async Task<bool> HasOverlapAsync(
        Guid listingId, Guid? listingUnitId, DateOnly start, DateOnly end, Guid? excludeRuleId, CancellationToken ct = default)
        => await _db.SeasonalPricingRules.AnyAsync(x =>
            x.ListingId == listingId
            && x.ListingUnitId == listingUnitId
            && x.IsActive
            && (excludeRuleId == null || x.Id != excludeRuleId)
            && x.StartDate <= end
            && x.EndDate >= start,
            ct);

    public async Task AddAsync(SeasonalPricingRule rule, CancellationToken ct = default)
    {
        await _db.SeasonalPricingRules.AddAsync(rule, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SeasonalPricingRule rule, CancellationToken ct = default)
    {
        _db.SeasonalPricingRules.Update(rule);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(SeasonalPricingRule rule, CancellationToken ct = default)
    {
        _db.SeasonalPricingRules.Remove(rule);
        await _db.SaveChangesAsync(ct);
    }
}
