namespace Ube.Application.DTOs
{
    public class VendorRegisterApplicationDto
    {
        // ======================
        // STEP 1 - Business Info
        // ======================
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public string Address { get; set; } = string.Empty;

        // ======================
        // STEP 2 - Contact Info
        // ======================
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;


        // STEP 3 (🔥 IMPORTANT)
        public List<string> Categories { get; set; } = new();
       

        public bool IsSubmitted { get; set; } = true;
    }
}