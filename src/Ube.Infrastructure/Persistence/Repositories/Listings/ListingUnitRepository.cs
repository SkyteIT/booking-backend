using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingUnitRepository : IListingUnitRepository
{
    private readonly ApplicationDbContext _db;

    public ListingUnitRepository(ApplicationDbContext db) => _db = db;

    public async Task<ListingUnit?> GetByIdAsync(Guid unitId, CancellationToken ct = default)
        => await _db.ListingUnits.FirstOrDefaultAsync(u => u.Id == unitId, ct);

    // Batched lookup for callers resolving several units by id at once
    // (e.g. multi-item checkout) instead of one round trip per id.
    public async Task<List<ListingUnit>> GetByIdsAsync(IEnumerable<Guid> unitIds, CancellationToken ct = default)
        => await _db.ListingUnits.Where(u => unitIds.Contains(u.Id)).ToListAsync(ct);

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

    public async Task DeleteByListingAndKindAsync(Guid listingId, ListingUnitKind kind, CancellationToken ct = default)
    {
        var existing = await _db.ListingUnits
            .Where(u => u.ListingId == listingId && u.Kind == kind)
            .ToListAsync(ct);

        if (existing.Count == 0) return;

        _db.ListingUnits.RemoveRange(existing);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ListingUnitCleanupResult> CleanupDuplicatesAsync(CancellationToken ct = default)
    {
        var allUnits = await _db.ListingUnits.ToListAsync(ct);

        var bookedUnitIds = await _db.Bookings
            .Where(b => b.ListingUnitId != null)
            .Select(b => b.ListingUnitId!.Value)
            .Distinct()
            .ToListAsync(ct);
        var bookedSet = bookedUnitIds.ToHashSet();

        var groups = allUnits.GroupBy(u => new
        {
            u.ListingId,
            u.Kind,
            u.RowIndex,
            u.ColumnIndex,
            u.SlotStartTime,
            u.Code,
            u.Name
        });

        var toDelete = new List<ListingUnit>();
        var skippedBooked = new List<Guid>();

        foreach (var group in groups)
        {
            if (group.Count() <= 1) continue;

            // Deterministic pick: lowest DisplayOrder, then earliest Id -
            // the actual choice among identical duplicates doesn't matter,
            // consistency across repeated runs does.
            var ordered = group.OrderBy(u => u.DisplayOrder).ThenBy(u => u.Id).ToList();

            foreach (var duplicate in ordered.Skip(1))
            {
                if (bookedSet.Contains(duplicate.Id))
                    skippedBooked.Add(duplicate.Id);
                else
                    toDelete.Add(duplicate);
            }
        }

        if (toDelete.Count > 0)
        {
            _db.ListingUnits.RemoveRange(toDelete);
            await _db.SaveChangesAsync(ct);
        }

        return new ListingUnitCleanupResult
        {
            DeletedCount = toDelete.Count,
            SkippedUnitIdsWithBookings = skippedBooked
        };
    }
}
