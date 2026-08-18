using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Notification;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Interfaces;

namespace Ube.Api.Controllers;

// Marks this as an API controller (auto validation, better responses)
[ApiController]
[Authorize]

// Base route: api/notifications
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    // Service layer dependency (business logic)
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    // Constructor injection
    public NotificationController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // GET: api/notifications/user/{userId}
    // Get all notifications for a specific user
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var result = await _service.GetByUserIdAsync(userId, cancellationToken);
        return Ok(result); // 200 OK with data
    }

    [HttpGet("user/{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var count = await _service.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new { unreadCount = count });
    }

    // POST: api/notifications
    // Create a new notification
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return Ok(result); // 200 OK with created notification
    }

    // PUT: api/notifications/{id}/read
    // Mark a single notification as read
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        // The service verifies the notification owner before changing state.
        var updated = await _service.MarkAsReadAsync(id, cancellationToken);

        // If found and updated → 204 No Content
        // If not found → 404 Not Found
        return updated ? NoContent() : NotFound();
    }

    // PUT: api/notifications/user/{userId}/read-all
    // Mark all notifications of a user as read
    [HttpPut("user/{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var count = await _service.MarkAllAsReadAsync(userId, cancellationToken);

        // Return number of updated notifications
        return Ok(new { updatedCount = count });
    }

    // GET: api/notifications/preferences/{userId}
    // Get notification preferences (email, SMS, push)
    [HttpGet("preferences/{userId:guid}")]
    public async Task<IActionResult> GetPreferences(Guid userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var result = await _service.GetPreferencesAsync(userId, cancellationToken);
        return Ok(result);
    }

    // PUT: api/notifications/preferences/{userId}
    // Update notification preferences
    [HttpPut("preferences/{userId:guid}")]
    public async Task<IActionResult> SavePreference(
        Guid userId,
        [FromBody] UpdateNotificationPreferenceDto dto,
        CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var result = await _service.SavePreferenceAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    private bool CanAccessUser(Guid userId)
        => userId == _currentUser.UserId || User.IsInRole("Admin");
}
