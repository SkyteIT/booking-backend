using Ube.Domain.Enums.Listings;

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

    //  NEW — Listing Type
    public ListingType Type { get; set; }

    //  NEW — Type-specific data
    public HotelDetailsResponse? HotelDetails { get; set; }
}