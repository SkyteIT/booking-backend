using Ube.Domain.Enums.Bookings;

namespace Ube.Api.Contracts.Bookings;

public class UpdateVendorBookingStatusRequest
{
    public Guid VendorId { get; set; }
    public BookingStatus NewStatus { get; set; }
}