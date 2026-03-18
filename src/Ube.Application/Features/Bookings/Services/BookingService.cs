using Ube.Application.common.Interfaces.Services;
using Ube.Application.Features.Bookings.Rules;
using Ube.Domain.Enums.Bookings;
using Ube.Application.common.Interfaces.Persistence;
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
}
