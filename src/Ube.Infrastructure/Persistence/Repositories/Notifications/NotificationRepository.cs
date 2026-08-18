using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Notifications;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Enums.Notifications;

namespace Ube.Infrastructure.Persistence.Repositories.Notifications;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;

    public NotificationRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
        => await _db.Notifications.AddAsync(notification, ct);

    public async Task<IReadOnlyList<NotificationPreference>> GetPreferencesByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.NotificationPreferences
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.NotificationType)
            .ToListAsync(ct);

    public async Task<NotificationPreference?> GetPreferenceAsync(Guid userId, NotificationType type, CancellationToken ct = default)
        => await _db.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId && x.NotificationType == type, ct);

    public async Task AddPreferenceAsync(NotificationPreference preference, CancellationToken ct = default)
        => await _db.NotificationPreferences.AddAsync(preference, ct);

    public async Task<IReadOnlyList<PushSubscription>> GetPushSubscriptionsByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.PushSubscriptions
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

    public async Task<PushSubscription?> GetPushSubscriptionByEndpointAsync(string endpoint, CancellationToken ct = default)
        => await _db.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == endpoint, ct);

    public async Task AddPushSubscriptionAsync(PushSubscription subscription, CancellationToken ct = default)
        => await _db.PushSubscriptions.AddAsync(subscription, ct);

    public async Task RemovePushSubscriptionAsync(string endpoint, CancellationToken ct = default)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(x => x.Endpoint == endpoint, ct);
        if (existing != null)
            _db.PushSubscriptions.Remove(existing);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
