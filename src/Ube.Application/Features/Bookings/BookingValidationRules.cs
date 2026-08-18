using Ube.Domain.Enums.Bookings;
using Ube.Domain.Entities.Bookings;

namespace Ube.Application.Features.Bookings;

public static class BookingValidationRules
{
    //this is for vendor side validations
    public static Result BelongsToVendorListing(Booking booking, Guid currentVendorId)
    {
        if (booking.Listing.VendorProfile.UserId == currentVendorId)
            return Result.Success();
        else
            return Result.Failure("Booking does not belong to the specified vendor.");
    }
    public static Result CanVendorConfirm(Booking booking, Guid CurrentVendorId)
    {
        var belongsToVendor = BelongsToVendorListing(booking, CurrentVendorId);
        if (!belongsToVendor.IsSuccess)
            return belongsToVendor;

        if (BookingTransitionRules.CanVendorTransition(booking.Status, BookingStatus.Confirmed))
            return Result.Success();
        else
            return Result.Failure("Cannot confirm booking.");
    }
    public static Result CanVendorReject(Booking booking, Guid CurrentVendorId)
    {
        var belongsToVendor = BelongsToVendorListing(booking, CurrentVendorId);
        if (!belongsToVendor.IsSuccess)
            return belongsToVendor;

        if (BookingTransitionRules.CanVendorTransition(booking.Status, BookingStatus.Rejected))
            return Result.Success();
        else
            return Result.Failure("Cannot reject booking.");
    }
    // this part  for customer side validation 
    public static Result BelongsToUser(Booking booking, Guid currentUserId)
    {
        if (booking.CustomerId == currentUserId)
            return Result.Success();
        else
            return Result.Failure("Booking does not belong to the specified user.");
    }
    public static Result CanUserCancelPendding(Booking booking, Guid currentUserId)
    {
        var belongsToUser = BelongsToUser(booking, currentUserId);
        if (!belongsToUser.IsSuccess)
            return belongsToUser;

        if (booking.Status == BookingStatus.Pending && BookingTransitionRules.CanCustomerTransition(booking.Status, BookingStatus.Cancelled))
            return Result.Success();
        else
            return Result.Failure("Cannot cancel pending booking.");
    }
    public static Result CanUserCancelConfirmed(Booking booking, Guid currentUserId)
    {
        var belongsToUser = BelongsToUser(booking, currentUserId);
        if (!belongsToUser.IsSuccess)
            return belongsToUser;

        if (booking.Status == BookingStatus.Confirmed && BookingTransitionRules.CanCustomerTransition(booking.Status, BookingStatus.Cancelled))
            return Result.Success();
        else
            return Result.Failure("Cannot cancel confirmed booking.");
    }

    // this part for system side validation
    public static Result CanSystemComplete(Booking booking)
    {
        if (BookingTransitionRules.CanSystemTransition(booking.Status, BookingStatus.Completed))
            return Result.Success();
        else
            return Result.Failure("Cannot complete booking.");
    }
}