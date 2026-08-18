using Microsoft.AspNetCore.SignalR;
using Ube.Api.Hubs;
using Ube.Application.Common.Interfaces.Services;

namespace Ube.Api.Services;

public class SignalRRealtimeUpdateService : IRealtimeUpdateService
{
    private readonly IHubContext<RealtimeUpdatesHub> _hub;

    public SignalRRealtimeUpdateService(IHubContext<RealtimeUpdatesHub> hub)
    {
        _hub = hub;
    }

    public Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
        => _hub.Clients.Group($"user:{userId}").SendAsync(eventName, payload, cancellationToken);

    public Task PublishToRoleAsync(string role, string eventName, object payload, CancellationToken cancellationToken = default)
        => _hub.Clients.Group($"role:{role.ToLowerInvariant()}").SendAsync(eventName, payload, cancellationToken);
}
