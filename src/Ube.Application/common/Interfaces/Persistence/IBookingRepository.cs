using Ube.Domain.Entities.Bookings;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.common.Interfaces.Persistence;

public interface IBookingRepository
{
    Task<Booking ?> GetByIdAsync(Guid BookingId);
    Task UpdateAsync(Booking booking);
    Task <List<Booking>> GetBookingsByVendorIdAsync(Guid vendorId , BookingStatus? status);
    Task<int> GetNextBookingSequenceAsync();

    
}