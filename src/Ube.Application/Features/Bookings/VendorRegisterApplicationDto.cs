namespace Ube.Application.Features.Bookings;

public class VendorRegisterApplicationDto
{
    // Business Info
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string Address { get; set; } = string.Empty;

    // Contact Info
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = new();

    public int CurrentStep { get; set; } = 1;
}
