using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class Listing
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }  // renamed from VendorId
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public string? Tags { get; set; } // Comma separated
    public string? CancellationPolicy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ListingType Type { get; set; }

    // Navigation Properties
    public VendorProfile Vendor { get; set; } = default!;
    public Category Category { get; set; } = default!;
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
}