namespace Ube.Application.Features.Vendors;

public class UpdateVendorProfileDto
{
    // User fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Vendor fields
    public string BusinessName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? BusinessDescription { get; set; }
}