using Ube.Domain.Enums.Users;

namespace Ube.Domain.Entities.Users;

// A plain Admin's attempt to change another user's role doesn't mutate
// anything directly - it creates one of these (Pending), and only a
// SuperAdmin's approval actually applies CurrentRole -> RequestedRole.
// SuperAdmin itself bypasses this queue entirely (see AdminService).
public class RoleChangeRequest
{
    public Guid Id { get; set; }

    public Guid TargetUserId { get; set; }
    public User TargetUser { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = null!;

    // Snapshot at request time - the target's role may have moved on by
    // the time this is reviewed, but the request should show what it
    // looked like when asked for.
    public UserRole CurrentRole { get; set; }
    public UserRole RequestedRole { get; set; }
    public string? Reason { get; set; }

    public RoleChangeRequestStatus Status { get; set; } = RoleChangeRequestStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
