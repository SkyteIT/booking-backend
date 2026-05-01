using Ube.Application.Features.Reviews;
using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Reviews;
namespace Ube.Application.Features.Reviews;

public interface IReviewService
{
    Task CreateReviewAsync(CreateReviewDto dto, Guid currentUserId);

    Task<PagedResult<ReviewDto>> GetReviewsByVendorAsync(Guid vendorId, ReviewRequest request);
}
