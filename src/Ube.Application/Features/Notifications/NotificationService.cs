using Ube.Application.Features.Notifications.Email;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Enums.Notifications;

namespace Ube.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IPushService _pushService;

    public NotificationService(
        INotificationRepository repo,
        IEmailService emailService,
        ISmsService smsService,
        IPushService pushService)
    {
        _repo = repo;
        _emailService = emailService;
        _smsService = smsService;
        _pushService = pushService;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _repo.GetByUserIdAsync(userId, cancellationToken);
        return notifications.Select(ToDto).ToList();
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = (NotificationType)dto.Type,
            IsRead = false
        };

        await _repo.AddAsync(notification, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        var preference = await _repo.GetPreferenceAsync(dto.UserId, notification.Type, cancellationToken);

        if (preference?.EmailEnabled == true && !string.IsNullOrEmpty(dto.Email))
            await _emailService.SendEmailAsync(dto.Email, notification.Title, notification.Message);

        if (preference?.SmsEnabled == true && !string.IsNullOrEmpty(dto.PhoneNumber))
            await _smsService.SendSmsAsync(dto.PhoneNumber, notification.Message);

        if (preference?.PushEnabled == true)
        {
            var subscriptions = await _repo.GetPushSubscriptionsByUserIdAsync(dto.UserId, cancellationToken);
            foreach (var sub in subscriptions)
                await _pushService.SendPushAsync(sub.Endpoint, sub.P256dh, sub.Auth, notification.Title, notification.Message, cancellationToken);
        }

        return ToDto(notification);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _repo.GetByIdAsync(id, cancellationToken);
        if (notification is null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _repo.GetUnreadByUserIdAsync(userId, cancellationToken);

        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            n.UpdatedAt = DateTime.UtcNow;
        }

        await _repo.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var preferences = await _repo.GetPreferencesByUserIdAsync(userId, cancellationToken);
        return preferences.Select(x => new NotificationPreferenceDto
        {
            Id = x.Id,
            UserId = x.UserId,
            NotificationType = ((int)x.NotificationType).ToString(),
            EmailEnabled = x.EmailEnabled,
            PushEnabled = x.PushEnabled,
            SmsEnabled = x.SmsEnabled
        }).ToList();
    }

    public async Task<NotificationPreferenceDto> SavePreferenceAsync(Guid userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken)
    {
        var type = (NotificationType)dto.NotificationType;

        var preference = await _repo.GetPreferenceAsync(userId, type, cancellationToken);

        if (preference is null)
        {
            preference = new NotificationPreference
            {
                UserId = userId,
                NotificationType = type,
                EmailEnabled = dto.EmailEnabled,
                PushEnabled = dto.PushEnabled,
                SmsEnabled = dto.SmsEnabled
            };
            await _repo.AddPreferenceAsync(preference, cancellationToken);
        }
        else
        {
            preference.EmailEnabled = dto.EmailEnabled;
            preference.PushEnabled = dto.PushEnabled;
            preference.SmsEnabled = dto.SmsEnabled;
            preference.UpdatedAt = DateTime.UtcNow;
        }

        await _repo.SaveChangesAsync(cancellationToken);

        return new NotificationPreferenceDto
        {
            Id = preference.Id,
            UserId = preference.UserId,
            NotificationType = ((int)preference.NotificationType).ToString(),
            EmailEnabled = preference.EmailEnabled,
            PushEnabled = preference.PushEnabled,
            SmsEnabled = preference.SmsEnabled
        };
    }

    public async Task SubscribeToPushAsync(Guid userId, SubscribePushDto dto, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetPushSubscriptionByEndpointAsync(dto.Endpoint, cancellationToken);

        if (existing != null)
        {
            // Same browser endpoint re-subscribing (e.g. a different account
            // signed in on this device) - repoint it rather than duplicate.
            existing.UserId = userId;
            existing.P256dh = dto.P256dh;
            existing.Auth = dto.Auth;
        }
        else
        {
            await _repo.AddPushSubscriptionAsync(new PushSubscription
            {
                UserId = userId,
                Endpoint = dto.Endpoint,
                P256dh = dto.P256dh,
                Auth = dto.Auth
            }, cancellationToken);
        }

        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task UnsubscribeFromPushAsync(Guid userId, string endpoint, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetPushSubscriptionByEndpointAsync(endpoint, cancellationToken);
        if (existing == null || existing.UserId != userId)
            return;

        await _repo.RemovePushSubscriptionAsync(endpoint, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto ToDto(Notification x) => new()
    {
        Id = x.Id,
        UserId = x.UserId,
        Title = x.Title,
        Message = x.Message,
        Type = x.Type.ToString(),
        IsRead = x.IsRead,
        CreatedAt = x.CreatedAt,
        ReadAt = x.ReadAt
    };
}
