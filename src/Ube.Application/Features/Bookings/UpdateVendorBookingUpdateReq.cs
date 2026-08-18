using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Bookings;

public class UpdateVendorBookingStatusRequest
{
    public BookingStatus NewStatus { get; set; }
}