using Ube.Domain.Entities.Reviews;
using Ube.Application.Common.Models;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IReviewRepository
{
    Task AddAsync(Review review);

    Task<bool> ExistsByBookingIdAsync(Guid bookingId);

    Task<(List<Review> Items, int TotalCount)> GetPagedByVendorAsync(
        Guid vendorId,
        QueryOptions options
    );
}