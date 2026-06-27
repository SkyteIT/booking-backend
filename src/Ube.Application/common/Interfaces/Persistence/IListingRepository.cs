using Ube.Application.DTOs.Search;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Common.Interfaces.Persistence
{
    public interface IListingRepository
    {
        Task<Listing?> GetByIdAsync(Guid listingId);
        Task<List<Listing>> GetByVendorIdAsync(Guid vendorId);
        Task UpdateAsync(Listing listing);

        Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
        Task<List<Listing>> GetOrphanedByCategoryNameAsync(Guid uncategorizedId, string originalName, CancellationToken ct = default);

        Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken = default);
    }
}
