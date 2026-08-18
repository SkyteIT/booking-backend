using Ube.Application.Features.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Admin.Dashboard;

public interface IAdminService
{
    // Dashboard
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    // Users
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(Guid userId);

    // SuperAdmin: applies immediately. Plain Admin: creates a pending
    // RoleChangeRequest instead - see RoleChangeOutcomeDto.
    Task<RoleChangeOutcomeDto> UpdateUserRoleAsync(Guid userId, UserRole role, Guid actorUserId, string? reason = null, CancellationToken ct = default);
    Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isSuspended);

    // Bookings
    Task<List<AdminBookingDto>> GetAllBookingsAsync();
    Task<AdminBookingDto?> GetBookingByIdAsync(Guid bookingId);
    Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, string status);
    Task<byte[]> ExportBookingsCsvAsync();
}