namespace Ube.Application.DTOs.Listings;

public sealed class CreateListingRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }
    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
}