using Ube.Domain.Enums.Vendors;
namespace Ube.Application.Features.Admin.VendorApplications;
public class ApplicationTableDto
{
    public Guid Id { get; set; }
    public string userName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public VendorApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    
}
            