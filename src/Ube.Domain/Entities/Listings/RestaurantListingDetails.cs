namespace Ube.Domain.Entities.Listings;

public class RestaurantListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public string CuisineType { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;

    public Listing Listing { get; set; } = default!;
}