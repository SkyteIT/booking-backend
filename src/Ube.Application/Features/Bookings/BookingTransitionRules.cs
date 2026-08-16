
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;

public static class BookingTransitionRules
{
    // to check if a transition from currentStatus to newStatus 
    public static bool CanTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            // how pending status can change
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Rejected) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,

            // how confirmed status can change
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Completed) => true,

            //everything else invalid
            _ => false 
        };
    }
    // specific rules for vendor transitions
    public static bool CanVendorTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        if (!CanTransition(currentStatus, newStatus))
            return false;

        return (currentStatus, newStatus) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Rejected) => true,

            _ => false
        };
    }
    // specific rules for customer transitions
    public static bool CanCustomerTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        if(!CanTransition(currentStatus,newStatus))
            return false;
        return (currentStatus, newStatus) switch
        {
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,

            _ =>false
        };
    }
    // specific rules for system transitions (e.g., auto-complete after end time)
    public static bool CanSystemTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        if (!CanTransition(currentStatus, newStatus))
            return false;
        return (currentStatus, newStatus) switch
        {
            (BookingStatus.Confirmed,BookingStatus.Completed)=>true,
            _ => false
        };
    }
    // Admin can perform any transition that's structurally valid in the state
    // machine (unlike vendor/customer, not narrowed to a role-specific subset)
    // - but still bounded by CanTransition, so an admin can't jump e.g.
    // Rejected -> Confirmed or Completed -> Pending. Broad, not unchecked.
    public static bool CanAdminTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return CanTransition(currentStatus, newStatus);
    }
}