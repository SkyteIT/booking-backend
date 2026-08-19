using Ube.Domain.Entities.Bookings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Admin.Dashboard;

public interface IAdminRepository
{
    // Users
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task UpdateUserAsync(User user);

    // How many users currently hold any of the given roles - used to
    // guard against demoting the last remaining Admin/SuperAdmin.
    Task<int> CountByRolesAsync(IEnumerable<UserRole> roles);

    // All users currently holding any of the given roles - used to fan
    // out admin/finance alert notifications (see IAdminAlertService).
    Task<List<User>> GetByRolesAsync(IEnumerable<UserRole> roles);

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