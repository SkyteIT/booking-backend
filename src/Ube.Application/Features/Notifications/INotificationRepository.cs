using Ube.Domain.Entities.Notifications;
using Ube.Domain.Enums.Notifications;

namespace Ube.Application.Features.Notifications;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);

    Task<IReadOnlyList<NotificationPreference>> GetPreferencesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationPreference?> GetPreferenceAsync(Guid userId, NotificationType type, CancellationToken ct = default);
    Task AddPreferenceAsync(NotificationPreference preference, CancellationToken ct = default);

    Task<IReadOnlyList<PushSubscription>> GetPushSubscriptionsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<PushSubscription?> GetPushSubscriptionByEndpointAsync(string endpoint, CancellationToken ct = default);
    Task AddPushSubscriptionAsync(PushSubscription subscription, CancellationToken ct = default);
    Task RemovePushSubscriptionAsync(string endpoint, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
