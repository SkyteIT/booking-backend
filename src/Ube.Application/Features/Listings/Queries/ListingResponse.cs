using Ube.Domain.Enums.Listings;
using Ube.Application.Features.Listings.Commands;

namespace Ube.Application.Features.Listings.Queries;

public class ListingResponse
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public string Currency { get; set; } = "LKR";

    public string? Location { get; set; }

    public bool IsActive { get; set; }

    // Flattened fields
    public string CategoryName { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    //  NEW  Listing Type
    public ListingType Type { get; set; }

    public string Status { get; set; } = "Live";

    public double Rating { get; set; }

    public int BookingsCount { get; set; }

    public string? PrimaryImage { get; set; }

    //  NEW  Type-specific data
    public List<string> Images { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string? CancellationPolicy { get; set; }

    public HotelDetailsDto? HotelDetails { get; set; }
    public RestaurantDetailsDto? RestaurantDetails { get; set; }
    public EventDetailsDto? EventDetails { get; set; }
    public CarRentalDetailsDto? CarRentalDetails { get; set; }
    public ActivityDetailsDto? ActivityDetails { get; set; }
}
