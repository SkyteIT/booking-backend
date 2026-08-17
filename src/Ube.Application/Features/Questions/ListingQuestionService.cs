using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;
using Ube.Domain.Entities.Questions;

namespace Ube.Application.Features.Questions;

public class ListingQuestionService : IListingQuestionService
{
    private readonly IListingQuestionRepository _questionRepo;
    private readonly IListingRepository _listingRepo;

    public ListingQuestionService(IListingQuestionRepository questionRepo, IListingRepository listingRepo)
    {
        _questionRepo = questionRepo;
        _listingRepo = listingRepo;
    }

    public async Task AskQuestionAsync(Guid listingId, AskQuestionDto dto, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(dto.QuestionText))
            throw new BusinessRuleException("Question text is required");

        // GetByIdAsync already includes VendorProfile, so this stamps
        // VendorId without a second query.
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new NotFoundException("Listing not found");

        var question = new ListingQuestion
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            CustomerId = customerId,
            VendorId = listing.VendorProfile.UserId,
            QuestionText = dto.QuestionText.Trim()
        };

        await _questionRepo.AddAsync(question);
    }

    public async Task<PagedResult<QuestionDto>> GetQuestionsByListingAsync(Guid listingId, QuestionRequest request)
    {
        var (items, totalCount) = await _questionRepo.GetPagedByListingAsync(listingId, request);
        return ToPagedDto(items, totalCount, request);
    }

    public async Task<PagedResult<QuestionDto>> GetQuestionsByVendorAsync(Guid vendorId, QuestionRequest request)
    {
        var (items, totalCount) = await _questionRepo.GetPagedByVendorAsync(vendorId, request);
        return ToPagedDto(items, totalCount, request);
    }

    private static PagedResult<QuestionDto> ToPagedDto(List<ListingQuestion> items, int totalCount, QuestionRequest request)
    {
        var mapped = items.Select(q => new QuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            AnswerText = q.AnswerText,
            AnsweredAt = q.AnsweredAt,
            CreatedAt = q.CreatedAt,
            CustomerName = q.Customer.FirstName + " " + q.Customer.LastName
        }).ToList();

        return new PagedResult<QuestionDto>
        {
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task AnswerQuestionAsync(Guid questionId, AnswerQuestionDto dto, Guid vendorUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.AnswerText))
            throw new BusinessRuleException("Answer text is required");

        var question = await _questionRepo.GetByIdAsync(questionId)
            ?? throw new NotFoundException("Question not found");

        if (question.VendorId != vendorUserId)
            throw new ForbiddenException("Not your listing's question");

        question.AnswerText = dto.AnswerText.Trim();
        question.AnsweredAt = DateTime.UtcNow;

        await _questionRepo.UpdateAsync(question);
    }
}
