using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ube.Application.Features.Notifications;

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
    /// Notification type: 0 = General, 1 = Booking, 2 = Promotion, etc.
    /// </summary>
    [DefaultValue(0)]
    public int Type { get; set; }

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
