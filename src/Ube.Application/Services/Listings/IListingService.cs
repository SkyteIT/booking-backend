using Ube.Application.DTOs.Listings;

namespace Ube.Application.Services.Listings;

public interface IListingService
{
    Task<IReadOnlyList<ListingResponseDto>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<ListingResponseDto?> GetActiveListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListingResponseDto>> GetVendorListingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ListingResponseDto> CreateAsync(Guid userId, CreateListingRequest request, CancellationToken cancellationToken = default);
    Task<ListingResponseDto?> UpdateAsync(Guid userId, Guid listingId, CreateListingRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
}