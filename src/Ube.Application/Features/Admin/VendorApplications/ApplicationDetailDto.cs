using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Admin.VendorApplications;

public class ApplicationDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Business Info
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? TaxId { get; set; }
    public string Address { get; set; } = string.Empty;

    // Contact Info
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? Categories { get; set; }

    // Documents
    public string? BusinessLicensePath { get; set; }
    public string? InsuranceCertificatePath { get; set; }
    public string? TaxDocumentPath { get; set; }

    // Status & Review
    public VendorApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
