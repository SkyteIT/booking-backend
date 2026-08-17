using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;

namespace Ube.Application.Common.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDetailDto> UpdateVendorBookingStatusAsync(Guid bookingId, Guid vendorId, BookingStatus newStatus);
    Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId, BookingsRequest request);
    Task<BookingDetailDto> GetBookingDetailAsync(Guid bookingId, Guid vendorId);

    Task<PagedResult<VendorBookingDto>> GetCustomerBookingsAsync(Guid customerId, BookingsRequest request);
    Task<BookingDetailDto> GetCustomerBookingDetailAsync(Guid bookingId, Guid customerId);
    Task<BookingDetailDto> CancelBookingAsync(Guid bookingId, Guid customerId);
}


/// var nextValue = await _bookingRepository.GetNextBookingSequenceAsync();
/// var bookingNumber = $"BKG-{nextValue:D6}";

