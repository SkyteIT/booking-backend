namespace Ube.Application.Features.Notifications;

public interface IPushService
{
    Task SendPushAsync(string endpoint, string p256dh, string auth, string title, string message, CancellationToken ct = default);
}
