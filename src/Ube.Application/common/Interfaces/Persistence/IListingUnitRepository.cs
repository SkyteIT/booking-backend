using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

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
    // Used before a bulk grid/time-slot generation so re-running it
    // replaces the previous batch instead of layering duplicates on top.
    Task DeleteByListingAndKindAsync(Guid listingId, ListingUnitKind kind, CancellationToken ct = default);

    // One-time cleanup for duplicate units created before
    // DeleteByListingAndKindAsync existed (see AddGridAsync/
    // AddTimeSlotsAsync) - within each (ListingId, Kind, RowIndex,
    // ColumnIndex, SlotStartTime, Code, Name) group, keeps one unit and
    // removes the rest, except any duplicate a real booking already
    // references. Safe to call more than once.
    Task<ListingUnitCleanupResult> CleanupDuplicatesAsync(CancellationToken ct = default);
}

public class ListingUnitCleanupResult
{
    public int DeletedCount { get; set; }
    public List<Guid> SkippedUnitIdsWithBookings { get; set; } = new();
}
