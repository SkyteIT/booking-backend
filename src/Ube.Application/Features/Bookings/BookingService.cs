using Ube.Application.Common.Interfaces.Services;
using Ube.Domain.Enums.Bookings;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models.Pagination;

namespace Ube.Application.Features.Bookings;

public class BookingService : IBookingService
{
    public readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }
    // Method for vendor to update booking status
    public async Task<BookingDetailDto?> UpdateVendorBookingStatusAsync(Guid BookingId, Guid VendorId, BookingStatus newStatus)
    {
        //retireve the booking
        var booking = await _bookingRepository.GetByIdAsync(BookingId);

        if (booking == null)
            return null;

        var isAllowed = newStatus switch
        {
            BookingStatus.Confirmed => 
                BookingValidationRules.CanVendorConfirm(booking, VendorId).IsSuccess,
            BookingStatus.Rejected => 
                BookingValidationRules.CanVendorReject(booking , VendorId).IsSuccess,
            
            _ => false
        };
        if (!isAllowed)
            return null;

        booking.Status = newStatus;

        await _bookingRepository.UpdateAsync(booking);
        
        return new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = booking.Listing.Title,
            CustomerName = booking.Customer.FirstName + " " + booking.Customer.LastName,
            CustomerEmail = booking.Customer.Email,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = BookingValidationRules.CanVendorConfirm(booking, VendorId).IsSuccess,
            CanReject = BookingValidationRules.CanVendorReject(booking, VendorId).IsSuccess
        };
    }
    // Method to get bookings for a vendor with pagination and optional status filter
    public async Task<PagedResult<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId, BookingsRequest request)
    {
        var bookings = await _bookingRepository.GetBookingsByVendorIdAsync(vendorId, request);
        var vendorBookings = bookings.Items.Select(b => new VendorBookingDto
        {   
            BookingId = b.Id,
            BookingNumber = b.BookingNumber,
            ListingTitle = b.Listing.Title,
            CustomerName = b.Customer.FirstName + " " + b.Customer.LastName,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Status = b.Status,
            TotalAmount = b.TotalAmount,
            Currency = b.Currency,
            CreatedAt = b.CreatedAt
        }).ToList();
        return new PagedResult<VendorBookingDto>
        {
            Items = vendorBookings,
            PageNumber = bookings.PageNumber,
            PageSize = bookings.PageSize,
            TotalCount = bookings.TotalCount,
            TotalPages = bookings.TotalPages
        };
    }

    public async Task<BookingDetailDto?> GetBookingDetailAsync(Guid BookingId, Guid vendorId)
    {
        var booking = await _bookingRepository.GetBookingAsync(BookingId, vendorId);
        if(booking ==null)
            return null;
        
        return new BookingDetailDto
        {
            BookingId = booking.Id,
            BookingNumber = booking.BookingNumber,
            ListingTitle = booking.Listing.Title,
            CustomerName = booking.Customer.FirstName + " " + booking.Customer.LastName,
            CustomerEmail = booking.Customer.Email,
            StartDateTime = booking.StartDateTime,
            EndDateTime = booking.EndDateTime,
            Status = booking.Status,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            CreatedAt = booking.CreatedAt,
            CanConfirm = BookingValidationRules.CanVendorConfirm(booking, vendorId).IsSuccess,
            CanReject = BookingValidationRules.CanVendorReject(booking, vendorId).IsSuccess
        };
    }
}
