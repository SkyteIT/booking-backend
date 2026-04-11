using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Notification;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Notifications;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _context;

    public NotificationService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type.ToString(),
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc,
                ReadAtUtc = x.ReadAtUtc
            })
            .ToListAsync(cancellationToken);
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

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
            ReadAtUtc = notification.ReadAtUtc
        };
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notification is null) return false;

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;
        notification.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            notification.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.NotificationPreferences
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.NotificationType)
            .Select(x => new NotificationPreferenceDto
            {
                Id = x.Id,
                UserId = x.UserId,
                NotificationType = x.NotificationType.ToString(),
                EmailEnabled = x.EmailEnabled,
                PushEnabled = x.PushEnabled,
                SmsEnabled = x.SmsEnabled
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationPreferenceDto> SavePreferenceAsync(Guid userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken)
    {
        var type = (NotificationType)dto.NotificationType;

        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId && x.NotificationType == type, cancellationToken);

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

            _context.NotificationPreferences.Add(preference);
        }
        else
        {
            preference.EmailEnabled = dto.EmailEnabled;
            preference.PushEnabled = dto.PushEnabled;
            preference.SmsEnabled = dto.SmsEnabled;
            preference.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
}
