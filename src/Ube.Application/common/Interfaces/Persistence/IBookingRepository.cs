using Ube.Domain.Entities.Bookings;
using Ube.Application.Features.Bookings.Requests;
using Ube.Application.common.Models.pagination;

namespace Ube.Application.common.Interfaces.Persistence;

public interface IBookingRepository
{
    Task<Booking ?> GetByIdAsync(Guid BookingId);
    Task UpdateAsync(Booking booking);
    Task<PagedResult<Booking>> GetBookingsByVendorIdAsync(Guid vendorId, BookingsRequest request);
    Task<Booking?>GetBookingAsync(Guid BookingId , Guid vendorId);
    Task<int> GetNextBookingSequenceAsync();

    
}