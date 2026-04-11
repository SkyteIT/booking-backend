namespace Ube.Application.DTOs.Notification;

public class UpdateNotificationPreferenceDto
{
    public int NotificationType { get; set; }
    public bool EmailEnabled { get; set; }
    public bool PushEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}
