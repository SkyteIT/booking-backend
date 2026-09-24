namespace Ube.Application.Features.Listings;

public interface IListingAddonService
{
    Task<IReadOnlyList<ListingAddonDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<ListingAddonDto> CreateAsync(Guid listingId, Guid userId, CreateListingAddonRequest request, CancellationToken ct = default);
    Task<ListingAddonDto> UpdateAsync(Guid listingId, Guid addonId, Guid userId, UpdateListingAddonRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid listingId, Guid addonId, Guid userId, CancellationToken ct = default);
}
