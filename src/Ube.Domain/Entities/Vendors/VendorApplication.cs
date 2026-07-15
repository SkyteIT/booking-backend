using Ube.Domain.Enums.Vendors;

namespace Ube.Domain.Entities.Vendors;

public class VendorApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
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

    // Categories (comma-separated)
    public string? Categories { get; set; }

    // Documents
    public string? BusinessLicensePath { get; set; }
    public string? InsuranceCertificatePath { get; set; }
    public string? TaxDocumentPath { get; set; }

    // Multi-step form tracking
    public int CurrentStep { get; set; } = 1;

    // Admin review workflow
    public VendorApplicationStatus Status { get; set; } = VendorApplicationStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
