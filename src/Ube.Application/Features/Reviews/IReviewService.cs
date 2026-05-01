using Ube.Application.Features.Reviews;
using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Reviews;
namespace Ube.Application.Features.Reviews;

public interface IReviewService
{
    Task CreeateReviewAsync(CreateReviewDto dto, Guid currentUserId);

    Task <PagedResult<ReviewDto>> GetReviewsForVendorAsync(Guid vendorId, int pageNumber, int pageSize);
}
