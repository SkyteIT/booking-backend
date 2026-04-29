using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Admin;
using Ube.Application.Services.Admin;
using Ube.Domain.Enums;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/admin/dashboard
    /// Returns platform statistics for the admin dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/admin/users
    /// Returns all users
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/admin/users/{userId}
    /// Returns a specific user
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<AdminUserDto>> GetUserById(Guid userId)
    {
        try
        {
            var user = await _adminService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = $"User {userId} not found." });

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/admin/users/{userId}/role
    /// Updates a user's role
    /// </summary>
    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var user = await _adminService.UpdateUserRoleAsync(userId, request.Role);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/admin/users/{userId}/status
    /// Suspends or reactivates a user
    /// </summary>
    [HttpPut("users/{userId}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var user = await _adminService.UpdateUserStatusAsync(userId, request.IsSuspended);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/admin/bookings
    /// Returns all bookings
    /// </summary>
    [HttpGet("bookings")]
    public async Task<ActionResult<List<AdminBookingDto>>> GetAllBookings()
    {
        try
        {
            var bookings = await _adminService.GetAllBookingsAsync();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/admin/bookings/{bookingId}
    /// Returns a specific booking
    /// </summary>
    [HttpGet("bookings/{bookingId}")]
    public async Task<ActionResult<AdminBookingDto>> GetBookingById(Guid bookingId)
    {
        try
        {
            var booking = await _adminService.GetBookingByIdAsync(bookingId);
            if (booking == null)
                return NotFound(new { message = $"Booking {bookingId} not found." });

            return Ok(booking);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT api/admin/bookings/{bookingId}/status
    /// Updates a booking status
    /// </summary>
    [HttpPut("bookings/{bookingId}/status")]
    public async Task<ActionResult<AdminBookingDto>> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request)
    {
        try
        {
            var booking = await _adminService.UpdateBookingStatusAsync(bookingId, request.Status);
            return Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// ── Request Models ────────────────────────────────────────────────────────────

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsSuspended { get; set; }
}

public class UpdateBookingStatusRequest
{
    public string Status { get; set; } = string.Empty;
}