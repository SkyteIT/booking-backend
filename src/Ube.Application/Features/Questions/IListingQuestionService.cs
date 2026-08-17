using Ube.Application.Common.Models.Pagination;

namespace Ube.Application.Features.Questions;

public interface IListingQuestionService
{
    Task AskQuestionAsync(Guid listingId, AskQuestionDto dto, Guid customerId);
    Task<PagedResult<QuestionDto>> GetQuestionsByListingAsync(Guid listingId, QuestionRequest request);
    Task<PagedResult<QuestionDto>> GetQuestionsByVendorAsync(Guid vendorId, QuestionRequest request);
    Task AnswerQuestionAsync(Guid questionId, AnswerQuestionDto dto, Guid vendorUserId);
}
