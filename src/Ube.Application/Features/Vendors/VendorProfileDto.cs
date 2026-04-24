namespace Ube.Application.Features.Vendors;

public class VendorProfileDto
{
    // User fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Vendor fields
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? BusinessDescription { get; set; }
}