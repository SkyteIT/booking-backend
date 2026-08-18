using Ube.Application.Features.Admin.Dashboard;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public class RoleChangeRequestDto
{
    public Guid Id { get; set; }
    public Guid TargetUserId { get; set; }
    public string TargetUserName { get; set; } = string.Empty;
    public string TargetUserEmail { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public UserRole CurrentRole { get; set; }
    public UserRole RequestedRole { get; set; }
    public string? Reason { get; set; }
    public RoleChangeRequestStatus Status { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Returned by AdminService.UpdateUserRoleAsync - exactly one of the two
// is populated, telling the caller which path happened: SuperAdmin
// applied it now, or a plain Admin's attempt became a pending request.
public class RoleChangeOutcomeDto
{
    public bool AppliedImmediately { get; set; }
    public AdminUserDto? User { get; set; }
    public RoleChangeRequestDto? Request { get; set; }
}

public class RejectRoleChangeRequestRequest
{
    public string? Notes { get; set; }
}
