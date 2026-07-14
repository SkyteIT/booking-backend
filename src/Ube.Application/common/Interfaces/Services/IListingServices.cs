using Ube.Application.Features.Listings;

namespace Ube.Application.Common.Interfaces.Services;
public interface IListingService
{
    Task<Guid> CreateListingAsync(Guid userId, CreateListingRequest request, CancellationToken ct = default);
    Task UpdateListingAsync(Guid listingId, Guid userId, UpdateListingRequest request, CancellationToken ct = default);
    Task DeleteListingAsync(Guid listingId, Guid userId, CancellationToken ct = default);
    Task<ListingResponse?> GetListingByIdAsync(Guid listingId, CancellationToken ct = default);
    Task<List<ListingResponse>> GetAllListingsAsync(CancellationToken ct = default);
    Task<List<ListingResponse>> GetMyListingsAsync(Guid userId, CancellationToken ct = default);
}
