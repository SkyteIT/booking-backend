namespace Ube.Application.Features.Listings;

public interface IListingOfferService
{
    Task<IReadOnlyList<ListingOfferDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingOfferDto> CreateAsync(Guid listingId, Guid userId, CreateListingOfferRequest request, CancellationToken ct = default);
    Task<ListingOfferDto> UpdateAsync(Guid listingId, Guid offerId, Guid userId, UpdateListingOfferRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid listingId, Guid offerId, Guid userId, CancellationToken ct = default);
}
