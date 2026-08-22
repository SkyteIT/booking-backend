using System.ComponentModel.DataAnnotations;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings.Commands;

public class CreateListingRequest
{
    [Required]
    public Guid VendorId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public ListingType Type { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "LKR";

    [MaxLength(200)]
    public string? Location { get; set; }

    public string Status { get; set; } = "Active";
    public bool IsAvailable { get; set; } = true;
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }

    // Hotel-specific fields
    public HotelDetailsDto? HotelDetails { get; set; }

    public RestaurantDetailsDto? RestaurantDetails { get; set; }

    public EventDetailsDto? EventDetails { get; set; }

    public CarRentalDetailsDto? CarRentalDetails { get; set; }

    public ActivityDetailsDto? ActivityDetails { get; set; }
}