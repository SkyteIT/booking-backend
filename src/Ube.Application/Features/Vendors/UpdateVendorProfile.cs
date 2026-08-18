namespace Ube.Application.Features.Vendors;

public class UpdateVendorProfileDto
{
    // User fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Vendor fields
    public string? BusinessName { get; set; }
    public string? Bio { get; set; }
    
}