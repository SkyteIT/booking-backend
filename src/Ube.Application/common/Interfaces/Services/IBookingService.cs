using Ube.Domain.Enums.Bookings;

namespace Ube.Application.common.Interfaces.Services;

public interface IBookingService
{
    Task<bool> UpdateVendorBookingStatusAsync(Guid bookingId,Guid VendorId, BookingStatus newStatus);
}