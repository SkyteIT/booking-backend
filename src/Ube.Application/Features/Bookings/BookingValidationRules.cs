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
    public static bool BelongsToUser(Booking booking, Guid currentUserId)
    {
        if (booking.CustomerId == currentUserId)
            return true;
        else
            return false;
    }
    public static bool CanUserCancelPendding(Booking booking, Guid currentUserId)
    {
        return BelongsToUser(booking, currentUserId) &&
                booking.Status == BookingStatus.Pending &&
                BookingTransitionRules.CanCustomerTransition(booking.Status, BookingStatus.Cancelled);
    }
    public static bool CanUserCancelConfirmed(Booking booking, Guid currentUserId)
    {
        return BelongsToUser(booking, currentUserId) &&
                booking.Status == BookingStatus.Confirmed &&
                BookingTransitionRules.CanCustomerTransition(booking.Status, BookingStatus.Cancelled);
    }

    // this part for system side validation
    public static bool CanSystemComplete(Booking booking)
    {
        return BookingTransitionRules.CanSystemTransition(booking.Status, BookingStatus.Completed);
    }
}