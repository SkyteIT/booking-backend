using System.Text;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Users;
using Ube.Domain.Enums.Users;
using Ube.Domain.Enums.Bookings;

namespace Ube.Application.Features.Admin.Dashboard;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IRoleChangeRequestService _roleChangeRequestService;

    public AdminService(IAdminRepository adminRepository, IRoleChangeRequestService roleChangeRequestService)
    {
        _adminRepository = adminRepository;
        _roleChangeRequestService = roleChangeRequestService;
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

    public async Task<RoleChangeOutcomeDto> UpdateUserRoleAsync(Guid userId, UserRole role, Guid actorUserId, string? reason = null, CancellationToken ct = default)
    {
        var target = await _adminRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        var actor = await _adminRepository.GetUserByIdAsync(actorUserId)
            ?? throw new NotFoundException("Acting user not found.");

        if (actor.Role != UserRole.SuperAdmin)
        {
            // Nothing changes yet - a plain Admin's ask becomes a pending
            // request only a SuperAdmin can approve.
            var request = await _roleChangeRequestService.CreateAsync(userId, role, actorUserId, reason, ct);
            return new RoleChangeOutcomeDto { AppliedImmediately = false, Request = request };
        }

        var adminOrSuperAdminCount = await _adminRepository.CountByRolesAsync(new[] { UserRole.Admin, UserRole.SuperAdmin });
        var check = RoleChangeRules.CanChangeRole(actorUserId, target, role, adminOrSuperAdminCount);
        if (!check.IsSuccess)
            throw new BusinessRuleException(check.ErrorMessage);

        target.Role = role;
        target.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpdateUserAsync(target);
        await _adminRepository.SaveChangesAsync();

        return new RoleChangeOutcomeDto { AppliedImmediately = true, User = MapUserToDto(target) };
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

        var newStatus = status.ToLower() switch
        {
            "confirmed"  => BookingStatus.Confirmed,
            "rejected"   => BookingStatus.Rejected,
            "cancelled"  => BookingStatus.Cancelled,
            "completed"  => BookingStatus.Completed,
            "pending"    => BookingStatus.Pending,
            _ => throw new BusinessRuleException($"Invalid status: {status}")
        };

        // Routed through the same state machine every other role uses - an
        // admin has broader transitions available than vendor/customer, but
        // can't jump a booking to any status regardless of its current one.
        var transitionRule = BookingValidationRules.CanAdminSetStatus(booking, newStatus);
        if (!transitionRule.IsSuccess)
            throw new BusinessRuleException(transitionRule.ErrorMessage);

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        await _adminRepository.UpdateBookingAsync(booking);
        await _adminRepository.SaveChangesAsync();

        return MapBookingToDto(booking);
    }

    // Generates the full bookings export as CSV bytes, server-side - same
    // dataset GetAllBookingsAsync returns, just formatted as a downloadable
    // file instead of JSON. UTF-8 BOM prefix so Excel opens it correctly.
    public async Task<byte[]> ExportBookingsCsvAsync()
    {
        var bookings = await GetAllBookingsAsync();

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", new[]
        {
            "Booking ID", "Customer", "Customer Email", "Service", "Category",
            "Start", "End", "Amount", "Currency", "Status", "Created At"
        }.Select(CsvEscape)));

        foreach (var b in bookings)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                b.Id.ToString(),
                b.CustomerName,
                b.CustomerEmail,
                b.ListingTitle,
                b.ListingCategory,
                b.StartDateTime.ToString("O"),
                b.EndDateTime.ToString("O"),
                b.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                b.Currency,
                b.Status,
                b.CreatedAt.ToString("O"),
            }.Select(CsvEscape)));
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(content).ToArray();
    }

    // Wraps a field in quotes (and doubles any internal quotes) if it
    // contains a comma, quote, or newline - without this, a customer name
    // like "Smith, John" would silently split into two CSV columns.
    private static string CsvEscape(string field)
    {
        field ??= string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
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
