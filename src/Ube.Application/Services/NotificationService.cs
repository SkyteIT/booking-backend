using Ube.Application.DTOs.Notification;
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Enums.Notifications;

namespace Ube.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public NotificationService(
        INotificationRepository repo,
        IEmailService emailService,
        ISmsService smsService)
    {
        _repo = repo;
        _emailService = emailService;
        _smsService = smsService;
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
            NotificationType = x.NotificationType.ToString(),
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
            NotificationType = preference.NotificationType.ToString(),
            EmailEnabled = preference.EmailEnabled,
            PushEnabled = preference.PushEnabled,
            SmsEnabled = preference.SmsEnabled
        };
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
