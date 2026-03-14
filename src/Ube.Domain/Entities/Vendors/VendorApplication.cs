using Ube.Domain.Enums;

namespace Ube.Domain.Entities;

public class VendorApplication
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public VendorApplicationStatus Status { get; set; } = VendorApplicationStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }
}