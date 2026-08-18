namespace Ube.Application.Common.Interfaces.Services;

public interface IRealtimeUpdateService
{
    Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default);
    Task PublishToRoleAsync(string role, string eventName, object payload, CancellationToken cancellationToken = default);
}
