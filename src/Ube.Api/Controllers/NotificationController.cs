using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Notification;
using Ube.Application.Interfaces;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByUserIdAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _service.MarkAsReadAsync(id, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("user/{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId, CancellationToken cancellationToken)
    {
        var count = await _service.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new { updatedCount = count });
    }

    [HttpGet("preferences/{userId:guid}")]
    public async Task<IActionResult> GetPreferences(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _service.GetPreferencesAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("preferences/{userId:guid}")]
    public async Task<IActionResult> SavePreference(
        Guid userId,
        [FromBody] UpdateNotificationPreferenceDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.SavePreferenceAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}
