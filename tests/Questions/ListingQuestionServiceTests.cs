using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Questions;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Questions;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Tests.Questions;

public class ListingQuestionServiceTests
{
    private sealed record Ctx(
        Mock<IListingQuestionRepository> QuestionRepo,
        Mock<IListingRepository> ListingRepo,
        ListingQuestionService Service);

    private static Ctx Build()
    {
        var questionRepo = new Mock<IListingQuestionRepository>();
        var listingRepo = new Mock<IListingRepository>();
        return new Ctx(questionRepo, listingRepo, new ListingQuestionService(questionRepo.Object, listingRepo.Object));
    }

    // --- AskQuestionAsync ---

    [Fact]
    public async Task AskQuestion_Throws_BusinessRuleException_When_Text_Empty()
    {
        var ctx = Build();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ctx.Service.AskQuestionAsync(Guid.NewGuid(), new AskQuestionDto { QuestionText = "   " }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AskQuestion_Throws_NotFoundException_When_Listing_Missing()
    {
        var ctx = Build();
        ctx.ListingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Listing?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.AskQuestionAsync(Guid.NewGuid(), new AskQuestionDto { QuestionText = "Any pets allowed?" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AskQuestion_Stamps_VendorId_From_Listing_And_Adds()
    {
        var ctx = Build();
        var listingId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var vendorUserId = Guid.NewGuid();
        var listing = new Listing
        {
            Id = listingId,
            VendorProfile = new VendorProfile { UserId = vendorUserId }
        };
        ctx.ListingRepo.Setup(r => r.GetByIdAsync(listingId)).ReturnsAsync(listing);

        ListingQuestion? added = null;
        ctx.QuestionRepo
            .Setup(r => r.AddAsync(It.IsAny<ListingQuestion>()))
            .Callback<ListingQuestion>(q => added = q)
            .Returns(Task.CompletedTask);

        await ctx.Service.AskQuestionAsync(listingId, new AskQuestionDto { QuestionText = "  Any pets allowed?  " }, customerId);

        Assert.NotNull(added);
        Assert.Equal(vendorUserId, added!.VendorId);
        Assert.Equal(customerId, added.CustomerId);
        Assert.Equal(listingId, added.ListingId);
        Assert.Equal("Any pets allowed?", added.QuestionText);
        Assert.Null(added.AnswerText);
    }

    // --- AnswerQuestionAsync ---

    [Fact]
    public async Task AnswerQuestion_Throws_NotFoundException_When_Question_Missing()
    {
        var ctx = Build();
        ctx.QuestionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ListingQuestion?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.AnswerQuestionAsync(Guid.NewGuid(), new AnswerQuestionDto { AnswerText = "Yes" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AnswerQuestion_Throws_ForbiddenException_When_Not_Owning_Vendor()
    {
        var ctx = Build();
        var question = new ListingQuestion { Id = Guid.NewGuid(), VendorId = Guid.NewGuid() };
        ctx.QuestionRepo.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ctx.Service.AnswerQuestionAsync(question.Id, new AnswerQuestionDto { AnswerText = "Yes" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task AnswerQuestion_Sets_AnswerText_And_AnsweredAt_For_Owning_Vendor()
    {
        var ctx = Build();
        var vendorUserId = Guid.NewGuid();
        var question = new ListingQuestion { Id = Guid.NewGuid(), VendorId = vendorUserId, QuestionText = "Any pets allowed?" };
        ctx.QuestionRepo.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await ctx.Service.AnswerQuestionAsync(question.Id, new AnswerQuestionDto { AnswerText = "  Yes, small pets only.  " }, vendorUserId);

        Assert.Equal("Yes, small pets only.", question.AnswerText);
        Assert.NotNull(question.AnsweredAt);
        ctx.QuestionRepo.Verify(r => r.UpdateAsync(question), Times.Once);
    }

    // --- GetQuestionsByListingAsync ---

    [Fact]
    public async Task GetQuestionsByListing_Maps_Paged_Result()
    {
        var ctx = Build();
        var listingId = Guid.NewGuid();
        var question = new ListingQuestion
        {
            Id = Guid.NewGuid(),
            QuestionText = "Any pets allowed?",
            CreatedAt = DateTime.UtcNow,
            Customer = new User { FirstName = "Jane", LastName = "Doe" }
        };

        ctx.QuestionRepo
            .Setup(r => r.GetPagedByListingAsync(listingId, It.IsAny<QuestionRequest>()))
            .ReturnsAsync((new List<ListingQuestion> { question }, 1));

        var result = await ctx.Service.GetQuestionsByListingAsync(listingId, new QuestionRequest { PageNumber = 1, PageSize = 10 });

        Assert.Single(result.Items);
        Assert.Equal("Any pets allowed?", result.Items[0].QuestionText);
        Assert.Equal("Jane Doe", result.Items[0].CustomerName);
        Assert.Equal(1, result.TotalCount);
    }
}
