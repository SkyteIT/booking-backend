using Ube.Application.Common.Models;
using Ube.Domain.Entities.Questions;

namespace Ube.Application.Features.Questions;

public interface IListingQuestionRepository
{
    Task AddAsync(ListingQuestion question);
    Task<(List<ListingQuestion> Items, int TotalCount)> GetPagedByListingAsync(Guid listingId, QueryOptions options);
    Task<(List<ListingQuestion> Items, int TotalCount)> GetPagedByVendorAsync(Guid vendorId, QueryOptions options);
    Task<ListingQuestion?> GetByIdAsync(Guid id);
    Task UpdateAsync(ListingQuestion question);
}
