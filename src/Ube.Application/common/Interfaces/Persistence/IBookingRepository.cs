using Ube.Domain.Entities.Bookings;
using Ube.Application.Features.Bookings;
using Ube.Application.Common.Models.Pagination;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IBookingRepository
{
    Task<Booking ?> GetByIdAsync(Guid BookingId);
    Task UpdateAsync(Booking booking);
    Task<PagedResult<Booking>> GetBookingsByVendorIdAsync(Guid vendorId, BookingsRequest request);
    Task<List<Booking>> GetAllBookingsByVendorIdAsync(Guid vendorId);
    Task<Booking?>GetBookingAsync(Guid BookingId , Guid vendorId);
    Task<int> GetNextBookingSequenceAsync();

    Task <List<Booking>> GetBookingsByListingAndDateRangeAsync(Guid listingId , DateTime startDate , DateTime endDate);
    Task AddAsync(Booking booking);

    Task<PagedResult<Booking>> GetBookingsByCustomerIdAsync(
        Guid customerId,
        BookingsRequest request);

    Task<Booking?> GetCustomerBookingAsync(
        Guid bookingId,
        Guid customerId);
    Task<Domain.Entities.Listings.Listing?> GetByListingIdAsync(Guid listingId);

}