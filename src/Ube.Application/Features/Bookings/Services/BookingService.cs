using Ube.Application.common.Interfaces.Services;
using Ube.Application.Features.Bookings.Rules;
using Ube.Domain.Enums.Bookings;
using Ube.Application.common.Interfaces.Persistence;
using Ube.Application.Features.Bookings.DTOs;
using Ube.Domain.Entities.Bookings;
namespace Ube.Application.Features.Bookings.Services;

public class BookingService : IBookingService
{
    public readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }
    public async Task<bool> UpdateVendorBookingStatusAsync(Guid BookingId, Guid VendorId, BookingStatus newStatus)
    {
        //retireve the booking
        var booking = await _bookingRepository.GetByIdAsync(BookingId);

        if (booking == null)
            return false;

        var isAllowed = newStatus switch
        {
            BookingStatus.Confirmed => 
                BookingValidationRules.CanVendorConfirm(booking, VendorId),
            BookingStatus.Rejected => 
                BookingValidationRules.CanVendorReject(booking , VendorId),
            
            _ => false
        };
        if (!isAllowed)
            return false;

        booking.Status = newStatus;

        await _bookingRepository.UpdateAsync(booking);
        
        return true;
    }
    public async Task<List<VendorBookingDto>> GetVendorBookingsAsync(Guid vendorId, BookingStatus? status = null , BookingSortBy? sortBy = null)
    {
        var bookings = await _bookingRepository.GetBookingsByVendorIdAsync(vendorId, status, sortBy);
        return bookings.Select(b => new VendorBookingDto
        {
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
    }

    public async Task<BookingDetailDto?> GetBookingDetailAsync(Guid BookingId, Guid vendorId)
    {
        var booking = await _bookingRepository.GetBookingAsync(BookingId, vendorId);
        if(booking ==null)
            return null;
        
        return new BookingDetailDto
        {
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
            CanConfirm = BookingValidationRules.CanVendorConfirm(booking, vendorId),
            CanReject = BookingValidationRules.CanVendorReject(booking, vendorId)
        };
    }
}
