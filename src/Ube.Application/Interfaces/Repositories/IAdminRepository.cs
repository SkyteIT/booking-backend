using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Users;

namespace Ube.Application.Interfaces.Repositories;

public interface IAdminRepository
{
    // Users
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task UpdateUserAsync(User user);

    // Bookings
    Task<List<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    Task UpdateBookingAsync(Booking booking);

    // Dashboard stats
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetTotalBookingsCountAsync();
    Task<int> GetActiveBookingsCountAsync();
    Task<int> GetPendingBookingsCountAsync();
    Task<int> GetCancelledBookingsCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<int> GetTotalListingsCountAsync();
    Task<int> GetActiveListingsCountAsync();
    Task<int> GetTotalVendorsCountAsync();

    Task SaveChangesAsync();
}