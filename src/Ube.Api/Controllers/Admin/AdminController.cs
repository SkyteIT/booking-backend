using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Admin.Dashboard;

namespace Ube.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<ActionResult<AdminUserDto>> GetUserById(Guid userId)
    {
        var user = await _adminService.GetUserByIdAsync(userId);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("users/{userId:guid}/role")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await _adminService.UpdateUserRoleAsync(userId, request.Role);
        return Ok(user);
    }

    [HttpPut("users/{userId:guid}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _adminService.UpdateUserStatusAsync(userId, request.IsSuspended);
        return Ok(user);
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    [HttpGet("bookings")]
    public async Task<ActionResult<List<AdminBookingDto>>> GetAllBookings()
    {
        var bookings = await _adminService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("bookings/{bookingId:guid}")]
    public async Task<ActionResult<AdminBookingDto>> GetBookingById(Guid bookingId)
    {
        var booking = await _adminService.GetBookingByIdAsync(bookingId);
        return booking == null ? NotFound() : Ok(booking);
    }

    [HttpPut("bookings/{bookingId:guid}/status")]
    public async Task<ActionResult<AdminBookingDto>> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request)
    {
        var booking = await _adminService.UpdateBookingStatusAsync(bookingId, request.Status);
        return Ok(booking);
    }
}
