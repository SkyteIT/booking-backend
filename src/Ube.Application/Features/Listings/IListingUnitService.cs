using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Listings;

public interface IListingUnitService
{
    Task<IReadOnlyList<ListingUnitDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingUnitDto> AddAsync(Guid listingId, Guid userId, AddListingUnitRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ListingUnitDto>> AddGridAsync(Guid listingId, Guid userId, AddListingUnitsGridRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ListingUnitDto>> AddTimeSlotsAsync(Guid listingId, Guid userId, AddListingUnitsTimeSlotsRequest request, CancellationToken ct = default);
    Task<ListingUnitDto> UpdateAsync(Guid listingId, Guid unitId, Guid userId, UpdateListingUnitRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid listingId, Guid unitId, Guid userId, CancellationToken ct = default);
    // Public - backs seat-map "already taken" display on the customer page.
    Task<IReadOnlyList<Guid>> GetBookedUnitIdsAsync(Guid listingId, DateTime start, DateTime end, CancellationToken ct = default);
    // Admin-only one-time cleanup for duplicate units created before the
    // grid/time-slot generation bug was fixed. See ListingUnitCleanupResult.
    Task<ListingUnitCleanupResult> CleanupDuplicatesAsync(CancellationToken ct = default);
}
