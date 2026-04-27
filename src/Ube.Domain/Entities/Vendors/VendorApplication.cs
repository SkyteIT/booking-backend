using Ube.Domain.Enums.Vendors;
namespace Ube.Domain.Entities.Vendors;

using Ube.Domain.Entities.Users;

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




    public User User { get; set; } = default!;
}