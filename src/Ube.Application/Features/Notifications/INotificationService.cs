namespace Ube.Application.Features.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken);
    Task<NotificationPreferenceDto> SavePreferenceAsync(Guid userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken);
}
