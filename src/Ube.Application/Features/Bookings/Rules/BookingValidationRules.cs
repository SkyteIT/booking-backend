using Ube.Domain.Enums.Bookings;
using Ube.Domain.Entities.Bookings;

namespace Ube.Application.Features.Bookings.Rules;

public static class BookingValidationRules
{
    //this is for vendor side validations
    public static bool BelongsToVendorListing(Booking booking, Guid currentVendorId)
    {
        return booking.Listing.VendorProfileId == currentVendorId;
    }
    public static bool CanVendorConfirm(Booking booking, Guid CurrentVendorId)
    {
        return BelongsToVendorListing(booking, CurrentVendorId) && 
                BookingTransitionRules.CanVendorTransition(booking.Status , BookingStatus.Confirmed);
    }
    public static bool CanVendorReject(Booking booking, Guid CurrentVendorId)
    {
        return BelongsToVendorListing(booking, CurrentVendorId) &&
                BookingTransitionRules.CanVendorTransition(booking.Status, BookingStatus.Rejected);
    }
    // this part  for customer side validation 
    public static bool BelongsToUser(Booking booking, Guid currentUserId)
    {
        return booking.CustomerId == currentUserId;
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