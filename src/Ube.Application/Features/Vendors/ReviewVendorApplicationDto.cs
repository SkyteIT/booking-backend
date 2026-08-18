using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Vendors;
public class ReviewVendorApplicationDto
{
    public VendorApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}