using Ube.Domain.Enums.Bookings;
using Ube.Application.Features.Bookings.DTOs;
using Ube.Application.common.Models.pagination;
using Ube.Application.Features.Bookings.Requests;


namespace Ube.Application.common.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDetailDto?> UpdateVendorBookingStatusAsync(Guid bookingId,Guid VendorId, BookingStatus newStatus);
    Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId , BookingsRequest request);
    Task <BookingDetailDto?> GetBookingDetailAsync(Guid BookingId , Guid vendorId);
}


/// var nextValue = await _bookingRepository.GetNextBookingSequenceAsync();
/// var bookingNumber = $"BKG-{nextValue:D6}";

