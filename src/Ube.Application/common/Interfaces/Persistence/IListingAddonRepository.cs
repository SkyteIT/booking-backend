using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IListingAddonRepository
{
    Task<ListingAddon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ListingAddon>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task AddAsync(ListingAddon addon, CancellationToken cancellationToken = default);
    Task UpdateAsync(ListingAddon addon, CancellationToken cancellationToken = default);
    Task DeleteAsync(ListingAddon addon, CancellationToken cancellationToken = default);
}
