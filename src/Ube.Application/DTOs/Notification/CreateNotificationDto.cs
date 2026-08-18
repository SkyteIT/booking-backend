using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ube.Application.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [DefaultValue("Notification Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DefaultValue("Notification message body.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// NotificationType enum value.
    /// </summary>
    [DefaultValue(101)]
    public int Type { get; set; }

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
