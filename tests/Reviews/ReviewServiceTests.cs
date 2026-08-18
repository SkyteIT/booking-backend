using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Reviews;
using Ube.Domain.Entities.Reviews;

namespace Ube.Tests.Reviews;

public class ReviewServiceTests
{
    private sealed record Ctx(
        Mock<IBookingRepository> BookingRepo,
        Mock<IReviewRepository> ReviewRepo,
        Mock<IListingRepository> ListingRepo,
        Mock<INotificationService> NotificationService,
        Mock<IUnitOfWork> UnitOfWork,
        ReviewService Service);

    private static Ctx Build()
    {
        var bookingRepo = new Mock<IBookingRepository>();
        var reviewRepo = new Mock<IReviewRepository>();
        var listingRepo = new Mock<IListingRepository>();
        var notificationService = new Mock<INotificationService>();
        var uow = new Mock<IUnitOfWork>();
        var ratingHelper = new RatingHelper(listingRepo.Object);
        return new Ctx(
            bookingRepo,
            reviewRepo,
            listingRepo,
            notificationService,
            uow,
            new ReviewService(bookingRepo.Object, reviewRepo.Object, ratingHelper, notificationService.Object, uow.Object));
    }

    private static Review MakeReview(Guid id) => new Review
    {
        Id = id,
        BookingId = Guid.NewGuid(),
        ListingId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        VendorId = Guid.NewGuid(),
        Rating = 5,
        Comment = "Great stay",
        CreatedAt = DateTime.UtcNow
    };

    // --- ToggleLikeAsync ---

    [Fact]
    public async Task ToggleLike_Throws_NotFoundException_When_Review_Missing()
    {
        var ctx = Build();
        ctx.ReviewRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            ctx.Service.ToggleLikeAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleLike_Likes_And_Returns_Updated_Count()
    {
        var ctx = Build();
        var review = MakeReview(Guid.NewGuid());
        var customerId = Guid.NewGuid();

        ctx.ReviewRepo.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
        ctx.ReviewRepo.Setup(r => r.ToggleLikeAsync(review.Id, customerId)).ReturnsAsync(true);
        ctx.ReviewRepo
            .Setup(r => r.GetLikeCountsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(review.Id))))
            .ReturnsAsync(new Dictionary<Guid, int> { [review.Id] = 3 });

        var (likeCount, isLiked) = await ctx.Service.ToggleLikeAsync(review.Id, customerId);

        Assert.True(isLiked);
        Assert.Equal(3, likeCount);
        ctx.ReviewRepo.Verify(r => r.ToggleLikeAsync(review.Id, customerId), Times.Once);
    }

    [Fact]
    public async Task ToggleLike_Unlikes_And_Returns_Zero_When_No_Likes_Remain()
    {
        var ctx = Build();
        var review = MakeReview(Guid.NewGuid());
        var customerId = Guid.NewGuid();

        ctx.ReviewRepo.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
        ctx.ReviewRepo.Setup(r => r.ToggleLikeAsync(review.Id, customerId)).ReturnsAsync(false);
        ctx.ReviewRepo
            .Setup(r => r.GetLikeCountsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var (likeCount, isLiked) = await ctx.Service.ToggleLikeAsync(review.Id, customerId);

        Assert.False(isLiked);
        Assert.Equal(0, likeCount);
    }

    // --- GetReviewsByListingAsync (like hydration) ---

    [Fact]
    public async Task GetReviewsByListing_Hydrates_LikeCount_And_IsLiked_For_Signed_In_Viewer()
    {
        var ctx = Build();
        var listingId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var review = MakeReview(Guid.NewGuid());
        review.Customer = new Domain.Entities.Users.User { FirstName = "Jane", LastName = "Doe" };

        ctx.ReviewRepo
            .Setup(r => r.GetPagedByListingAsync(listingId, It.IsAny<ReviewRequest>()))
            .ReturnsAsync((new List<Review> { review }, 1));
        ctx.ReviewRepo
            .Setup(r => r.GetLikeCountsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [review.Id] = 2 });
        ctx.ReviewRepo
            .Setup(r => r.GetLikedReviewIdsAsync(currentUserId, It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new HashSet<Guid> { review.Id });

        var result = await ctx.Service.GetReviewsByListingAsync(listingId, new ReviewRequest { PageNumber = 1, PageSize = 10 }, currentUserId);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].LikeCount);
        Assert.True(result.Items[0].IsLikedByCurrentUser);
    }

    [Fact]
    public async Task GetReviewsByListing_Skips_LikedLookup_For_Anonymous_Viewer()
    {
        var ctx = Build();
        var listingId = Guid.NewGuid();
        var review = MakeReview(Guid.NewGuid());
        review.Customer = new Domain.Entities.Users.User { FirstName = "Jane", LastName = "Doe" };

        ctx.ReviewRepo
            .Setup(r => r.GetPagedByListingAsync(listingId, It.IsAny<ReviewRequest>()))
            .ReturnsAsync((new List<Review> { review }, 1));
        ctx.ReviewRepo
            .Setup(r => r.GetLikeCountsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [review.Id] = 5 });

        var result = await ctx.Service.GetReviewsByListingAsync(listingId, new ReviewRequest { PageNumber = 1, PageSize = 10 }, currentUserId: null);

        Assert.Equal(5, result.Items[0].LikeCount);
        Assert.False(result.Items[0].IsLikedByCurrentUser);
        ctx.ReviewRepo.Verify(r => r.GetLikedReviewIdsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }
}
