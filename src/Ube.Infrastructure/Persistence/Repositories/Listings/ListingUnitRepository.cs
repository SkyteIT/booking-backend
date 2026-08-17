using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingUnitRepository : IListingUnitRepository
{
    private readonly ApplicationDbContext _db;

    public ListingUnitRepository(ApplicationDbContext db) => _db = db;

    public async Task<ListingUnit?> GetByIdAsync(Guid unitId, CancellationToken ct = default)
        => await _db.ListingUnits.FirstOrDefaultAsync(u => u.Id == unitId, ct);

    public async Task<List<ListingUnit>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default)
        => await _db.ListingUnits
            .Where(u => u.ListingId == listingId && u.IsActive)
            .OrderBy(u => u.DisplayOrder)
            .ToListAsync(ct);

    public async Task AddAsync(ListingUnit unit, CancellationToken ct = default)
    {
        await _db.ListingUnits.AddAsync(unit, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ListingUnit> units, CancellationToken ct = default)
    {
        await _db.ListingUnits.AddRangeAsync(units, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ListingUnit unit, CancellationToken ct = default)
    {
        _db.ListingUnits.Update(unit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ListingUnit unit, CancellationToken ct = default)
    {
        _db.ListingUnits.Remove(unit);
        await _db.SaveChangesAsync(ct);
    }
}
