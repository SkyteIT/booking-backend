using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Interfaces.Repositories;

public interface IListingRepository
{
    Task<List<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<Listing?> GetActiveListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<List<Listing>> GetVendorListingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Listing?> GetOwnedListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);
    Task<VendorProfile?> GetVendorProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task DeleteAsync(Listing listing, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}