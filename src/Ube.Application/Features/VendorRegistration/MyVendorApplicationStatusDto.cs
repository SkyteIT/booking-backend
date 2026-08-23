using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.VendorRegistration;

public class MyVendorApplicationStatusDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public VendorApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool CanReapply { get; set; }
    public bool CanAccessVendorPortal { get; set; }
    public bool ShowRejectionMessageBeforeForm { get; set; }
}
