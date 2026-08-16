using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Bookings;

namespace Ube.Application.Common.Interfaces.Services;

public interface IBookingService
{
    // Vendor
    Task<BookingDetailDto> UpdateVendorBookingStatusAsync(
        Guid bookingId,
        Guid vendorId,
        BookingStatus newStatus);

    Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(
        Guid vendorId,
        BookingsRequest request);

    Task<BookingDetailDto> GetBookingDetailAsync(
        Guid bookingId,
        Guid vendorId);

    // Customer
    Task<BookingDetailDto> CreateBookingAsync(
        Guid customerId,
        CreateBookingRequest request);

    Task<PagedResult<CustomerBookingDto>> GetCustomerBookingsAsync(
        Guid customerId,
        BookingsRequest request);

    Task<BookingDetailDto> GetCustomerBookingDetailAsync(
        Guid bookingId,
        Guid customerId);

    Task<BookingDetailDto> CancelCustomerBookingAsync(
        Guid bookingId,
        Guid customerId);
}