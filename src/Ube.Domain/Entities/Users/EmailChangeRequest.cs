using Ube.Domain.Enums.Users;

namespace Ube.Domain.Entities.Users;

// Changing your own email is never self-service, regardless of role -
// this queues the ask (Pending) and only a SuperAdmin's approval actually
// mutates User.Email. Unlike RoleChangeRequest, the requester and the
// target are always the same account.
public class EmailChangeRequest
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Snapshot at request time - the account's email may have moved on
    // (a prior request approved) by the time this is reviewed.
    public string CurrentEmail { get; set; } = string.Empty;
    public string RequestedEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public EmailChangeRequestStatus Status { get; set; } = EmailChangeRequestStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
