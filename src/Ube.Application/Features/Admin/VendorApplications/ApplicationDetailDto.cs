using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Admin.VendorApplications;
public class ApplicationDetailDto
{
        public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Business details
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Contact details
    public string Address { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;

    // Documents
    public string BusinessLicenseUrl { get; set; } = string.Empty;
    public string InsurenceCertificateUrl { get; set; } = string.Empty;
    public string TaxDocumentUrl { get; set; } = string.Empty;

    // Status
    public VendorApplicationStatus Status { get; set; }

    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }

    public string? RejectionReason { get; set; }
}