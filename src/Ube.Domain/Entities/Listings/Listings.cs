using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class Listing
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }

    public VendorProfile VendorProfile { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "LKR";

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public AvailabilityType AvailabilityType { get; set; }

    public int Capacity { get; set; } // For Capacity-based availability

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
}