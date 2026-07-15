using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class Listing
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }
    public VendorProfile VendorProfile { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? OriginalCategoryName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public string? ThumbnailUrl { get; set; }

    public AvailabilityType AvailabilityType { get; set; } = AvailabilityType.Capacity;
    public int Capacity { get; set; }

    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Category-specific listing type (Hotel, Restaurant, Event, CarRental, Activity)
    public ListingType Type { get; set; }
    public string? Tags { get; set; } // Comma separated
    public string? CancellationPolicy { get; set; }

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();

    // One of these is populated depending on Type
    public HotelListingDetails? HotelDetails { get; set; }
    public RestaurantListingDetails? RestaurantDetails { get; set; }
    public EventListingDetails? EventDetails { get; set; }
    public CarRentalListingDetails? CarRentalDetails { get; set; }
    public ActivityListingDetails? ActivityDetails { get; set; }
}
