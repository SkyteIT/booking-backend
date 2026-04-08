using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;

namespace Ube.Application.Common.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDetailDto?> UpdateVendorBookingStatusAsync(Guid bookingId,Guid VendorId, BookingStatus newStatus);
    Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId , BookingsRequest request);
    Task <BookingDetailDto?> GetBookingDetailAsync(Guid BookingId , Guid vendorId);
}


/// var nextValue = await _bookingRepository.GetNextBookingSequenceAsync();
/// var bookingNumber = $"BKG-{nextValue:D6}";

