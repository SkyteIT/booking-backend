using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Admin.Dashboard;

public interface IAdminService
{
    // Dashboard
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    // Users
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(Guid userId);
    Task<AdminUserDto> UpdateUserRoleAsync(Guid userId, UserRole role);
    Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isSuspended);

    // Bookings
    Task<List<AdminBookingDto>> GetAllBookingsAsync();
    Task<AdminBookingDto?> GetBookingByIdAsync(Guid bookingId);
    Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, string status);
}