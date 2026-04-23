namespace Ube.Domain.Entities
{
    public class VendorRegisterApplication
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Step 1 - Business Info
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public string Address { get; set; }

        // Step tracking
        public int CurrentStep { get; set; } = 1;

        // Status
        public bool IsSubmitted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Step 2 - Contact Info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; }  = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // STEP 3 (🔥 NEW)
        public string? Categories { get; set; }

        public string? BusinessLicensePath { get; set; }
        public string? InsuranceCertificatePath { get; set; }
        public string? TaxDocumentPath { get; set; }
    }
}