using System.Linq;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Models;
using Ube.Application.Common.Interfaces;
using Ube.Domain.Entities.Reviews;

using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IReviewRepository _reviewRepo;

    public ReviewService(IBookingRepository bookingRepo, IListingRepository listingRepo, IReviewRepository reviewRepo)
    {
        _bookingRepo = bookingRepo;
        _listingRepo = listingRepo;
        _reviewRepo = reviewRepo;
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
}
