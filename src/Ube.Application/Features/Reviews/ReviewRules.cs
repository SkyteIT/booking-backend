using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Reviews;
public class ReviewRules
{
    //booking must be completed
    public static Result MustBeCompleted(BookingStatus status)
    {
        if(status != BookingStatus.Completed)
        {
            return Result.Failure("Booking must be completed to leave a review");
        }
        return Result.Success();
    }
    //only booking owner can review 
    public static Result MustBeBookingOwner(Guid bookingCustomerId, Guid userId)
    {
        if(bookingCustomerId != userId)
        {
            return Result.Failure("Only booking owner can leave a review");
        }
        return Result.Success();
    }
    // only one review perBooking
    public static Result CannotReviewTwice(bool reviewExists)
    {
        if(reviewExists)
        {
            return Result.Failure("You have already reviewed this booking");
        }
        return Result.Success();
    }
    // rating must be between 1 and 5
    public static Result ValidateRating(int rating)
    {
        if(rating < 1 || rating > 5)
        {
            return Result.Failure("Rating must be between 1 and 5");
        }
        return Result.Success();
    }
    //prevent reviewOwn business
    public static Result PreventReviewOwnBusiness(Guid vendorId, Guid currentUserId)
    {
        if(vendorId == currentUserId)
        {
            return Result.Failure("You cannot review your own business");
        }
        return Result.Success();
    }
}