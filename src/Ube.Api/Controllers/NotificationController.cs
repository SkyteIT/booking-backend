using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Notifications;

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

    // GET: api/notifications/mine
    // Get all notifications for the authenticated user
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _service.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
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

    // PUT: api/notifications/read-all
    // Mark all notifications of the authenticated user as read
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var count = await _service.MarkAllAsReadAsync(_currentUser.UserId, cancellationToken);

        // Return number of updated notifications
        return Ok(new { updatedCount = count });
    }

    // GET: api/notifications/preferences
    // Get notification preferences (email, SMS, push) for the authenticated user
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var result = await _service.GetPreferencesAsync(_currentUser.UserId, cancellationToken);
        return Ok(result);
    }

    // PUT: api/notifications/preferences
    // Update notification preferences for the authenticated user
    [HttpPut("preferences")]
    public async Task<IActionResult> SavePreference(
        [FromBody] UpdateNotificationPreferenceDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.SavePreferenceAsync(_currentUser.UserId, dto, cancellationToken);
        return Ok(result);
    }

    // POST: api/notifications/push/subscribe
    // Register (or repoint) a browser push subscription for the authenticated user
    [HttpPost("push/subscribe")]
    public async Task<IActionResult> SubscribeToPush([FromBody] SubscribePushDto dto, CancellationToken cancellationToken)
    {
        await _service.SubscribeToPushAsync(_currentUser.UserId, dto, cancellationToken);
        return NoContent();
    }

    // POST: api/notifications/push/unsubscribe
    // Remove a browser push subscription (only if it belongs to the caller)
    [HttpPost("push/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromPush([FromBody] UnsubscribePushDto dto, CancellationToken cancellationToken)
    {
        await _service.UnsubscribeFromPushAsync(_currentUser.UserId, dto.Endpoint, cancellationToken);
        return NoContent();
    }

    private bool CanAccessUser(Guid userId)
        => userId == _currentUser.UserId || User.IsInRole("Admin");
}
