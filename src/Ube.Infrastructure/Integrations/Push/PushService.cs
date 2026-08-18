using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ube.Application.Features.Notifications;
using WebPush;

namespace Ube.Infrastructure.Integrations.Push;

public class PushService : IPushService
{
    private readonly VapidDetails _vapidDetails;
    private readonly INotificationRepository _repo;
    private readonly ILogger<PushService> _logger;

    public PushService(IConfiguration config, INotificationRepository repo, ILogger<PushService> logger)
    {
        var publicKey = config["Vapid:PublicKey"]
            ?? throw new InvalidOperationException("Vapid:PublicKey is not configured.");
        var privateKey = config["Vapid:PrivateKey"]
            ?? throw new InvalidOperationException("Vapid:PrivateKey is not configured.");
        var subject = config["Vapid:Subject"]
            ?? throw new InvalidOperationException("Vapid:Subject is not configured.");

        _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        _repo = repo;
        _logger = logger;
    }

    public async Task SendPushAsync(string endpoint, string p256dh, string auth, string title, string message, CancellationToken ct = default)
    {
        var client = new WebPushClient();
        var subscription = new PushSubscription(endpoint, p256dh, auth);
        var payload = JsonSerializer.Serialize(new { title, body = message });

        try
        {
            await client.SendNotificationAsync(subscription, payload, _vapidDetails);
        }
        catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
        {
            // The browser revoked or expired this subscription - stop trying to
            // reach it instead of failing on every future notification.
            await _repo.RemovePushSubscriptionAsync(endpoint, ct);
            await _repo.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {Endpoint}", endpoint);
        }
    }
}
