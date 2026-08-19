using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Admin.Dashboard;
using Ube.Application.Features.Users;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IAdminService adminService, ICurrentUserService currentUser)
    {
        _adminService = adminService;
        _currentUser = currentUser;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    // ── Vendors ───────────────────────────────────────────────────────────────

    [HttpGet("vendors")]
    public async Task<ActionResult<List<AdminVendorSummaryDto>>> GetAllVendors()
    {
        var vendors = await _adminService.GetAllVendorsAsync();
        return Ok(vendors);
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
    public async Task<ActionResult<RoleChangeOutcomeDto>> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        var outcome = await _adminService.UpdateUserRoleAsync(userId, request.Role, _currentUser.UserId, request.Reason, ct);
        return Ok(outcome);
    }

    [HttpPut("users/{userId:guid}/status")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _adminService.UpdateUserStatusAsync(userId, request.IsSuspended, _currentUser.UserId);
        return Ok(user);
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    [HttpGet("bookings")]
    public async Task<ActionResult<List<AdminBookingDto>>> GetAllBookings()
    {
        var bookings = await _adminService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("bookings/export")]
    public async Task<IActionResult> ExportBookings()
    {
        var csvBytes = await _adminService.ExportBookingsCsvAsync();
        return File(csvBytes, "text/csv", $"bookings-{DateTime.UtcNow:yyyy-MM-dd}.csv");
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
