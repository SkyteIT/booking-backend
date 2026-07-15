using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingResponse
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;

    public ListingType Type { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? PrimaryImage { get; set; }

    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }

    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }
}
