using System.Linq;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Models;
using Ube.Application.Common.Interfaces;
using Ube.Domain.Entities.Reviews;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Helpers;


namespace Ube.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly RatingHelper _ratingHelper;

    public ReviewService(
        IBookingRepository bookingRepo,
        IReviewRepository reviewRepo,
        RatingHelper ratingHelper)
    {
        _bookingRepo = bookingRepo;
        _reviewRepo = reviewRepo;
        _ratingHelper = ratingHelper;
    }

    public async Task CreateReviewAsync(CreateReviewDto dto, Guid currentUserId)
    {
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
    }

    public async Task<PagedResult<ReviewDto>> GetReviewsByVendorAsync(Guid vendorId, ReviewRequest request)
    {
        var (app,totalItems) = await _reviewRepo.GetPagedByVendorAsync(vendorId, request);
        var mapped = app.Select(r => new ReviewDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            CustomerName = r.Customer.FirstName + " " + r.Customer.LastName
        }).ToList();
        return new PagedResult<ReviewDto>{
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
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review == null)
            throw new NotFoundException("Review not found");
        if(review.CustomerId != currentUserId)
            throw new BusinessRuleException("You can only update your own reviews");
        var ratingRule = ReviewRules.ValidateRating(dto.Rating);
        if (!ratingRule.IsSuccess)
            throw new BusinessRuleException(ratingRule.ErrorMessage);
        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _reviewRepo.UpdateAsync(review);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        if (review.CustomerId != userId)
            throw new ForbiddenException("You can only delete your own review");

        await _reviewRepo.DeleteAsync(review);
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
    
}
