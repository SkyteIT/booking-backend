namespace Ube.Application.DTOs.Listings;

public sealed class ListingResponseDto
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string Type { get; set; } = "Activity";
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsFeatured { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PrimaryImage { get; set; }
    public IReadOnlyList<string> Images { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public string? CancellationPolicy { get; set; }
    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
}