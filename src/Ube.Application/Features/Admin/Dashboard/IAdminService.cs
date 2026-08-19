using Ube.Application.Features.Users;
using Ube.Application.Features.Vendors;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Admin.Dashboard;

public interface IAdminService
{
    // Dashboard
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    // Vendors (picker for admin-facing finance/payments workflows)
    Task<List<AdminVendorSummaryDto>> GetAllVendorsAsync();

    // Users
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto?> GetUserByIdAsync(Guid userId);

    // SuperAdmin: applies immediately. Plain Admin: creates a pending
    // RoleChangeRequest instead - see RoleChangeOutcomeDto.
    Task<RoleChangeOutcomeDto> UpdateUserRoleAsync(Guid userId, UserRole role, Guid actorUserId, string? reason = null, CancellationToken ct = default);
    // A plain Admin cannot suspend a SuperAdmin, and nobody can suspend
    // themselves - see RoleChangeRules.CanChangeStatus.
    Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isSuspended, Guid actorUserId);

    // Bookings
    Task<List<AdminBookingDto>> GetAllBookingsAsync();
    Task<AdminBookingDto?> GetBookingByIdAsync(Guid bookingId);
    Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, string status);
    Task<byte[]> ExportBookingsCsvAsync();
}