namespace Ube.Domain.Entities.Listings;

public class HotelListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Listing Listing { get; set; } = default!;
}