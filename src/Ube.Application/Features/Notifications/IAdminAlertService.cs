using Ube.Domain.Enums.Notifications;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Notifications;

// Fans a single alert out to every user holding any of the given roles -
// the "admin should notify everything if he might miss it" mechanism.
// Best-effort by design: a notification failure never blocks the
// business action that triggered it.
public interface IAdminAlertService
{
    Task NotifyRolesAsync(IEnumerable<UserRole> roles, string title, string message, NotificationType type, CancellationToken ct = default);
}
