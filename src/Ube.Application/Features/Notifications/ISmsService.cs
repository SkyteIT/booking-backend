namespace Ube.Application.Features.Notifications;

public interface ISmsService
{
    Task SendSmsAsync(string to, string message);
}