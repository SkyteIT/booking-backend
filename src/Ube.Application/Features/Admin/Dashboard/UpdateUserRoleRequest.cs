using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Admin.Dashboard;

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}
