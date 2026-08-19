using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Vendors;
public class ReviewVendorApplicationDto
{
    public string Status { get; set; } = string.Empty;
    public string? Action { get; set; }
    public string? RejectionReason { get; set; }
}
