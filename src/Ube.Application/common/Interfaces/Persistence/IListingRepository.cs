using Ube.Application.Features.Search;
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

        Task<Listing?> GetByIdWithDetailsAsync(Guid listingId, CancellationToken ct = default);
        Task<List<Listing>> GetAllWithDetailsAsync(CancellationToken ct = default);
        Task<List<Listing>> GetByVendorProfileIdWithDetailsAsync(Guid vendorProfileId, CancellationToken ct = default);
        Task AddAsync(Listing listing, CancellationToken ct = default);
        Task DeleteAsync(Listing listing, CancellationToken ct = default);
        Task ReplaceImagesAsync(Guid listingId, IEnumerable<string> imageUrls, CancellationToken ct = default);

        Task<TDetail?> GetDetailsAsync<TDetail>(Guid listingId, CancellationToken ct = default)
            where TDetail : class, IListingDetail;
        Task UpsertDetailsAsync<TDetail>(Guid listingId, TDetail details, CancellationToken ct = default)
            where TDetail : class, IListingDetail;
    }
}
