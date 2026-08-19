using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

public class EmailChangeRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CurrentEmail { get; set; } = string.Empty;
    public string RequestedEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public EmailChangeRequestStatus Status { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmailChangeRequestDto
{
    public string RequestedEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class RejectEmailChangeRequestRequest
{
    public string? Notes { get; set; }
}
