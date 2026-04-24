using Ube.Domain.Enums.Bookings;

namespace Ube.Api.Contracts.Bookings;

public class UpdateVendorBookingStatusRequest
{
    public BookingStatus NewStatus { get; set; }
}