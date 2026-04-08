
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;

public static class BookingTransitionRules
{
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
}