using Ube.Application.Features.Reviews;
using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Reviews;
namespace Ube.Application.Features.Reviews;

public interface IReviewService
{
    Task CreateReviewAsync(CreateReviewDto dto, Guid currentUserId);

    Task<PagedResult<ReviewDto>> GetReviewsByVendorAsync(Guid vendorId, ReviewRequest request);
    Task<PagedResult<ReviewDto>> GetReviewsByListingAsync(Guid listingId, ReviewRequest request);
    Task<PagedResult<CustomerReviewDto>> GetMyReviewsAsync(Guid customerId, ReviewRequest request);
    Task<object> GetRatingAsync(Guid vendorId);
    
    Task UpdateReviewAsync(CreateReviewDto dto, Guid currentUserId, Guid reviewId);
    Task DeleteReviewAsync(Guid reviewId, Guid currentUserId);

    Task AddVendorReplyAsync(Guid reviewId, VendorReplyDto dto, Guid currentUserId);

    Task<PagedResult<AdminReviewDto>> GetReviewsForModerationAsync(bool? isHidden, ReviewRequest request);
    Task HideReviewAsync(Guid reviewId, Guid adminUserId, string reason);
    Task UnhideReviewAsync(Guid reviewId, Guid adminUserId);
}
