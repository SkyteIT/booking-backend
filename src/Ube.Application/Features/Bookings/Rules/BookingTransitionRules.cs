using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings.Rules;

public static class BookingTransitionRules
{
    public static bool CanTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            // Pending
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Rejected) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,

            // Confirmed
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Completed) => true,

            // Everything else is not allowed
            _ => false
        };
    }
}