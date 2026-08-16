using Ube.Application.Common.Exceptions;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Admin.Dashboard;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    // ── Dashboard ────────────────────────────────────────────────────────────

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        return new DashboardStatsDto
        {
            TotalUsers         = await _adminRepository.GetTotalUsersCountAsync(),
            TotalBookings      = await _adminRepository.GetTotalBookingsCountAsync(),
            ActiveBookings     = await _adminRepository.GetActiveBookingsCountAsync(),
            PendingBookings    = await _adminRepository.GetPendingBookingsCountAsync(),
            CancelledBookings  = await _adminRepository.GetCancelledBookingsCountAsync(),
            TotalRevenue       = await _adminRepository.GetTotalRevenueAsync(),
            TotalListings      = await _adminRepository.GetTotalListingsCountAsync(),
            ActiveListings     = await _adminRepository.GetActiveListingsCountAsync(),
            TotalVendors       = await _adminRepository.GetTotalVendorsCountAsync(),
        };
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<List<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _adminRepository.GetAllUsersAsync();
        return users.Select(MapUserToDto).ToList();
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _adminRepository.GetUserByIdAsync(userId);
        return user == null ? null : MapUserToDto(user);
    }

    public async Task<AdminUserDto> UpdateUserRoleAsync(Guid userId, UserRole role)
    {
        var user = await _adminRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpdateUserAsync(user);
        await _adminRepository.SaveChangesAsync();

        return MapUserToDto(user);
    }

    public async Task<AdminUserDto> UpdateUserStatusAsync(Guid userId, bool isSuspended)
    {
        var user = await _adminRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        user.IsEmailVerified = !isSuspended;
        user.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpdateUserAsync(user);
        await _adminRepository.SaveChangesAsync();

        return MapUserToDto(user);
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    public async Task<List<AdminBookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _adminRepository.GetAllBookingsAsync();
        return bookings.Select(MapBookingToDto).ToList();
    }

    public async Task<AdminBookingDto?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _adminRepository.GetBookingByIdAsync(bookingId);
        return booking == null ? null : MapBookingToDto(booking);
    }

    public async Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, string status)
    {
        var booking = await _adminRepository.GetBookingByIdAsync(bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found.");

        booking.Status = status.ToLower() switch
        {
            "confirmed"  => BookingStatus.Confirmed,
            "cancelled"  => BookingStatus.Cancelled,
            "completed"  => BookingStatus.Completed,
            "pending"    => BookingStatus.Pending,
            _ => throw new BusinessRuleException($"Invalid status: {status}")
        };

        booking.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpdateBookingAsync(booking);
        await _adminRepository.SaveChangesAsync();

        return MapBookingToDto(booking);
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static AdminUserDto MapUserToDto(Domain.Entities.Users.User user)
    {
        return new AdminUserDto
        {
            Id            = user.Id,
            FullName      = $"{user.FirstName} {user.LastName}",
            Email         = user.Email,
            Role          = user.Role.ToString(),
            Status        = user.IsEmailVerified ? "Active" : "Suspended",
            PhoneNumber   = user.PhoneNumber,
            CreatedAt     = user.CreatedAt,
            TotalBookings = user.Bookings?.Count ?? 0,
        };
    }

    private static AdminBookingDto MapBookingToDto(Domain.Entities.Bookings.Booking booking)
    {
        return new AdminBookingDto
        {
            Id              = booking.Id,
            CustomerName    = $"{booking.Customer?.FirstName} {booking.Customer?.LastName}",
            CustomerEmail   = booking.Customer?.Email ?? string.Empty,
            ListingTitle    = booking.Listing?.Title ?? string.Empty,
            ListingCategory = booking.Listing?.Category?.Name ?? string.Empty,
            StartDateTime   = booking.StartDateTime,
            EndDateTime     = booking.EndDateTime,
            TotalAmount     = booking.TotalAmount,
            Currency        = booking.Currency,
            Status          = booking.Status.ToString(),
            CreatedAt       = booking.CreatedAt,
        };
    }
}
