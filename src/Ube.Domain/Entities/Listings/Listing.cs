using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class Listing
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "LKR";

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ListingType Type { get; set; }

    //  Navigation Properties
    public VendorProfile Vendor { get; set; } = default!;
    public Category Category { get; set; } = default!;

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();

}