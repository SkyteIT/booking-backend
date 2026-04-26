using System.ComponentModel.DataAnnotations;

namespace Ube.Application.Features.Listings.Commands;

public class UpdateListingRequest
{
    [Required]
    public Guid VendorId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Required]
    public string Currency { get; set; } = "LKR";

    public string? Location { get; set; }

    public bool IsActive { get; set; }
}