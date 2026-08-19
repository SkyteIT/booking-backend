using Ube.Application.Features.Admin.Dashboard;
using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Notifications;

public class AdminAlertService : IAdminAlertService
{
    private readonly IAdminRepository _adminRepo;
    private readonly INotificationService _notificationService;

    public AdminAlertService(IAdminRepository adminRepo, INotificationService notificationService)
    {
        _adminRepo = adminRepo;
        _notificationService = notificationService;
    }

    public async Task NotifyRolesAsync(IEnumerable<UserRole> roles, string title, string message, NotificationType type, CancellationToken ct = default)
    {
        List<Ube.Domain.Entities.Users.User> recipients;
        try
        {
            recipients = await _adminRepo.GetByRolesAsync(roles);
        }
        catch
        {
            // Best-effort - a lookup failure never blocks the business action that triggered this alert.
            return;
        }

        foreach (var recipient in recipients)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = recipient.Id,
                    Title = title,
                    Message = message,
                    Type = (int)type
                }, ct);
            }
            catch
            {
                // Best-effort - one recipient's notification failure never blocks the rest.
            }
        }
    }
}
