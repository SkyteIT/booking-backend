using Ube.Domain.Entities.Bookings;

namespace Ube.Application.common.Interfaces.Persistence;

public interface IBookingRepository
{
    Task<Booking ?> GetByIdAsync(Guid BookingId);
    Task UpdateAsync(Booking booking);

    
}