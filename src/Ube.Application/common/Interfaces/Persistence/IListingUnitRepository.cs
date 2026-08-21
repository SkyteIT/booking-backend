using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IListingUnitRepository
{
    Task<ListingUnit?> GetByIdAsync(Guid unitId, CancellationToken ct = default);
    Task<List<ListingUnit>> GetByIdsAsync(IEnumerable<Guid> unitIds, CancellationToken ct = default);
    Task<List<ListingUnit>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);
    Task AddAsync(ListingUnit unit, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ListingUnit> units, CancellationToken ct = default);
    Task UpdateAsync(ListingUnit unit, CancellationToken ct = default);
    Task DeleteAsync(ListingUnit unit, CancellationToken ct = default);
}
