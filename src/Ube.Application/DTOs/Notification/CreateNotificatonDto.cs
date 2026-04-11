namespace Ube.Application.DTOs.Notification;

public class CreateNotificationDto
{
	public Guid UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public int Type { get; set; }
}
