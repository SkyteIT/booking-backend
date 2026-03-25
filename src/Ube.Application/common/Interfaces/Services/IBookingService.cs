using Ube.Domain.Enums.Bookings;
using Ube.Application.Features.Bookings.DTOs;

namespace Ube.Application.common.Interfaces.Services;

public interface IBookingService
{
    Task<bool> UpdateVendorBookingStatusAsync(Guid bookingId,Guid VendorId, BookingStatus newStatus);
    Task<List<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId);
}


/// var nextValue = await _bookingRepository.GetNextBookingSequenceAsync();
/// var bookingNumber = $"BKG-{nextValue:D6}";

