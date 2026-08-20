using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ube.Api.Hubs;

[Authorize]
public class RealtimeUpdatesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        var role = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (!string.IsNullOrWhiteSpace(role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role.ToLowerInvariant()}");
        }

        await base.OnConnectedAsync();
    }
}
