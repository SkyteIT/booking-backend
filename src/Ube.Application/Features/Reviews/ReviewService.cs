
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Common.Exceptions;

using Ube.Domain.Entities.Reviews;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Helpers;
using Ube.Application.Features.Notifications;
using Ube.Domain.Enums.Notifications;
using Ube.Application.Common.Interfaces.Services;


namespace Ube.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly RatingHelper _ratingHelper;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeUpdateService _realtimeUpdateService;

    public ReviewService(
        IBookingRepository bookingRepo,
        IReviewRepository reviewRepo,
        RatingHelper ratingHelper,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IRealtimeUpdateService realtimeUpdateService)
    {
        _bookingRepo = bookingRepo;
        _reviewRepo = reviewRepo;
        _ratingHelper = ratingHelper;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _realtimeUpdateService = realtimeUpdateService;
    }

    public async Task CreateReviewAsync(CreateReviewDto dto, Guid currentUserId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try{

            // validate booking exists and belongs to user
            //1 get booking
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);
            if (booking == null )
                throw new NotFoundException("Booking not found");
            // 2 rules
            var completedRule = ReviewRules.MustBeCompleted(booking.Status);
            if (!completedRule.IsSuccess)
                throw new BusinessRuleException(completedRule.ErrorMessage);
            
            var ownerRule = ReviewRules.MustBeBookingOwner(booking.CustomerId, currentUserId);
            if (!ownerRule.IsSuccess)
                throw new BusinessRuleException(ownerRule.ErrorMessage);
            var ownBusinessRule = ReviewRules.PreventReviewOwnBusiness(booking.Listing.VendorProfile.UserId, currentUserId);
            if (!ownBusinessRule.IsSuccess)
                throw new BusinessRuleException(ownBusinessRule.ErrorMessage);
            var exists = await _reviewRepo.ExistsByBookingIdAsync(dto.BookingId);
            var duplicateRule = ReviewRules.CannotReviewTwice(exists);
            if (!duplicateRule.IsSuccess)
                throw new BusinessRuleException(duplicateRule.ErrorMessage);
            var ratingRule = ReviewRules.ValidateRating(dto.Rating);
            if (!ratingRule.IsSuccess)
                throw new BusinessRuleException(ratingRule.ErrorMessage);

            //create review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                ListingId = booking.ListingId,
                VendorId = booking.Listing.VendorProfile.UserId,
                CustomerId = booking.CustomerId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };
            await _reviewRepo.AddAsync(review);

            await _ratingHelper.UpdateListingRatingAsync(review.ListingId, null, review.Rating);
            await _reviewRepo.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            await TryNotifyReviewCreatedAsync(review);
            await PublishDashboardRefreshAsync(review.VendorId, review.Id);
        }
        catch{
                await _unitOfWork.RollbackAsync();
                throw;
        }
    }

    public async Task<PagedResult<ReviewDto>> GetReviewsByVendorAsync(Guid vendorId, ReviewRequest request, Guid? currentUserId = null)
    {
        var (app,totalItems) = await _reviewRepo.GetPagedByVendorAsync(vendorId, request);
        var mapped = await MapWithLikesAsync(app, currentUserId);
        return new PagedResult<ReviewDto>{
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
        };
    }
    public async Task<PagedResult<ReviewDto>> GetReviewsByListingAsync(Guid listingId, ReviewRequest request, Guid? currentUserId = null)
    {
        var (app, totalItems) = await _reviewRepo.GetPagedByListingAsync(listingId, request);
        var mapped = await MapWithLikesAsync(app, currentUserId);
        return new PagedResult<ReviewDto>{
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
        };
    }

    // Shared like-hydration for a page of reviews - two grouped queries
    // for the whole page (counts + the viewer's liked subset), never
    // per-review, so this never turns into an N+1.
    private async Task<List<ReviewDto>> MapWithLikesAsync(List<Review> reviews, Guid? currentUserId)
    {
        var ids = reviews.Select(r => r.Id).ToList();
        var likeCounts = await _reviewRepo.GetLikeCountsAsync(ids);
        var likedByMe = currentUserId.HasValue
            ? await _reviewRepo.GetLikedReviewIdsAsync(currentUserId.Value, ids)
            : new HashSet<Guid>();

        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            CustomerName = r.Customer.FirstName + " " + r.Customer.LastName,
            LikeCount = likeCounts.GetValueOrDefault(r.Id),
            IsLikedByCurrentUser = likedByMe.Contains(r.Id),
            VendorReply = r.VendorReply,
            VendorReplyAt = r.VendorReplyAt,
            ListingId = r.ListingId,
            ListingTitle = r.Listing?.Title ?? string.Empty
        }).ToList();
    }

    public async Task<(int LikeCount, bool IsLiked)> ToggleLikeAsync(Guid reviewId, Guid customerId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId)
            ?? throw new NotFoundException("Review not found");

        var isLiked = await _reviewRepo.ToggleLikeAsync(review.Id, customerId);
        var counts = await _reviewRepo.GetLikeCountsAsync(new[] { review.Id });
        return (counts.GetValueOrDefault(review.Id), isLiked);
    }

    // A customer's own reviews - "My Reviews" page
    public async Task<PagedResult<CustomerReviewDto>> GetMyReviewsAsync(Guid customerId, ReviewRequest request)
    {
        var (app, totalItems) = await _reviewRepo.GetPagedByCustomerAsync(customerId, request);
        var mapped = app.Select(r => new CustomerReviewDto
        {
            Id = r.Id,
            BookingId = r.BookingId,
            ListingId = r.ListingId,
            ListingTitle = r.Listing.Title,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            VendorReply = r.VendorReply,
            VendorReplyAt = r.VendorReplyAt
        }).ToList();
        return new PagedResult<CustomerReviewDto>{
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
        };
    }

    // Get average rating and total count for a vendor
    public async Task<object> GetRatingAsync(Guid vendorId)
    {
        var (avg, count) = await _reviewRepo.GetRatingAsync(vendorId);
        return new {
            AverageRating = Math.Round(avg, 2),
            TotalCount = count
        };
    }

    // Update review
    public async Task UpdateReviewAsync(CreateReviewDto dto, Guid currentUserId, Guid reviewId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                ?? throw new NotFoundException("Review not found");

            var booking = await _bookingRepo.GetByIdAsync(review.BookingId)
                ?? throw new NotFoundException("Booking not found");

            if (booking.CustomerId != currentUserId)
                throw new BusinessRuleException("You can only update your own reviews");

            var completedRule = ReviewRules.MustBeCompleted(booking.Status);
            if (!completedRule.IsSuccess)
                throw new BusinessRuleException(completedRule.ErrorMessage);

            var ratingRule = ReviewRules.ValidateRating(dto.Rating);
            if (!ratingRule.IsSuccess)
                throw new BusinessRuleException(ratingRule.ErrorMessage);

            var oldRating = review.Rating;
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _ratingHelper.UpdateListingRatingAsync(review.ListingId, oldRating, review.Rating);
            await _reviewRepo.UpdateAsync(review);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                ?? throw new NotFoundException("Review not found");

            var booking = await _bookingRepo.GetByIdAsync(review.BookingId)
                ?? throw new NotFoundException("Booking not found");

            if (booking.CustomerId != userId)
                throw new ForbiddenException("You can only delete your own review");

            await _ratingHelper.UpdateListingRatingAsync(review.ListingId, review.Rating, null);
            await _reviewRepo.DeleteAsync(review);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task AddVendorReplyAsync(Guid reviewId, VendorReplyDto dto, Guid vendorUserId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        // optional: validate vendor owns listing
        if (review.VendorId != vendorUserId)
            throw new ForbiddenException("Not your review");

        review.VendorReply = dto.Reply.Trim();
        review.VendorReplyAt = DateTime.UtcNow;

        await _reviewRepo.UpdateAsync(review);
    }

    // Admin moderation queue - includes hidden reviews, optionally filtered by hidden status
    public async Task<PagedResult<AdminReviewDto>> GetReviewsForModerationAsync(bool? isHidden, ReviewRequest request)
    {
        var (app, totalItems) = await _reviewRepo.GetPagedForModerationAsync(isHidden, request);
        var mapped = app.Select(r => new AdminReviewDto
        {
            Id = r.Id,
            ListingId = r.ListingId,
            VendorId = r.VendorId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            CustomerName = r.Customer.FirstName + " " + r.Customer.LastName,
            IsHidden = r.IsHidden,
            HiddenAt = r.HiddenAt,
            HideReason = r.HideReason
        }).ToList();
        return new PagedResult<AdminReviewDto>{
            Items = mapped,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
        };
    }

    // Hide a review from public view; a hidden review no longer counts toward the listing's rating.
    public async Task HideReviewAsync(Guid reviewId, Guid adminUserId, string reason)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                ?? throw new NotFoundException("Review not found");

            if (review.IsHidden)
                throw new BusinessRuleException("Review is already hidden");

            review.IsHidden = true;
            review.HiddenAt = DateTime.UtcNow;
            review.HiddenByUserId = adminUserId;
            review.HideReason = reason;

            // A hidden review's rating no longer counts toward the listing's aggregate.
            await _ratingHelper.UpdateListingRatingAsync(review.ListingId, review.Rating, null);
            await _reviewRepo.UpdateAsync(review);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // Restore a hidden review; its rating counts toward the listing's aggregate again.
    public async Task UnhideReviewAsync(Guid reviewId, Guid adminUserId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId)
                ?? throw new NotFoundException("Review not found");

            if (!review.IsHidden)
                throw new BusinessRuleException("Review is not hidden");

            review.IsHidden = false;
            review.HiddenAt = null;
            review.HiddenByUserId = null;
            review.HideReason = null;

            await _ratingHelper.UpdateListingRatingAsync(review.ListingId, null, review.Rating);
            await _reviewRepo.UpdateAsync(review);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task TryNotifyReviewCreatedAsync(Review review)
    {
        var notifications = new[]
        {
            new { UserId = review.VendorId, Type = NotificationType.VendorNewCustomerReview, Title = "New customer review", Message = $"You received a new {review.Rating}-star review." },
            new { UserId = review.CustomerId, Type = NotificationType.CustomerReviewSubmitted, Title = "Review submitted", Message = "Your review was submitted successfully." }
        };

        foreach (var item in notifications)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = item.UserId,
                    Title = item.Title,
                    Message = item.Message,
                    Type = (int)item.Type
                }, CancellationToken.None);
            }
            catch
            {
                // best-effort only
            }
        }
    }

    private async Task PublishDashboardRefreshAsync(Guid vendorId, Guid reviewId)
    {
        try
        {
            var payload = new
            {
                reason = "review.created",
                reviewId,
                vendorId
            };

            await _realtimeUpdateService.PublishToRoleAsync("vendor", "dashboard.refresh", payload);
            await _realtimeUpdateService.PublishToRoleAsync("admin", "dashboard.refresh", payload);
        }
        catch
        {
            // Realtime refresh is best-effort.
        }
    }
}
