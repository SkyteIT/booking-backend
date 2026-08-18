using Ube.Domain.Entities.Reviews;
using Ube.Application.Common.Models;

namespace Ube.Application.Features.Reviews;

public interface IReviewRepository
{
    Task AddAsync(Review review);

    Task<bool> ExistsByBookingIdAsync(Guid bookingId);
    Task<Review?> GetByBookingIdAsync(Guid bookingId);

    Task<(List<Review> Items, int TotalCount)> GetPagedByVendorAsync(
        Guid vendorId,
        QueryOptions options
    );
    Task<(List<Review> Items, int TotalCount)> GetPagedByListingAsync(
        Guid listingId,
        QueryOptions options
    );
    Task<(List<Review> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        QueryOptions options
    );
    Task<(double AverageRating, int TotalCount)> GetRatingAsync(Guid vendorId);
    Task<(List<Review> Items, int TotalCount)> GetPagedForModerationAsync(
        bool? isHidden,
        QueryOptions options
    );

    Task<Review?> GetByIdAsync(Guid id);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
    Task SaveChangesAsync();

    Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> reviewIds);
    Task<HashSet<Guid>> GetLikedReviewIdsAsync(Guid customerId, IEnumerable<Guid> reviewIds);
    // Returns the review's new "liked by this customer" state after toggling.
    Task<bool> ToggleLikeAsync(Guid reviewId, Guid customerId);
}