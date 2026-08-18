using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IListingOfferRepository
{
    Task<ListingOffer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ListingOffer>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);

    // The single active, discount-bearing offer for this listing right
    // now (StartDate <= today <= EndDate) - used by checkout and the
    // price-quote endpoint. At most one can exist, enforced at write time.
    Task<ListingOffer?> GetActiveDiscountForListingAsync(Guid listingId, DateOnly today, CancellationToken ct = default);

    // True if another active, discount-bearing offer for this listing
    // already overlaps this date range. Pure-perk offers (no DiscountType)
    // never count - they can run concurrently freely.
    Task<bool> HasActiveDiscountOverlapAsync(Guid listingId, DateOnly start, DateOnly end, Guid? excludeOfferId, CancellationToken ct = default);

    Task AddAsync(ListingOffer offer, CancellationToken ct = default);
    Task UpdateAsync(ListingOffer offer, CancellationToken ct = default);
    Task DeleteAsync(ListingOffer offer, CancellationToken ct = default);
}
