namespace Ube.Application.Features.Vendors;

public class AdminVendorSummaryDto
{
    public Guid VendorProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
