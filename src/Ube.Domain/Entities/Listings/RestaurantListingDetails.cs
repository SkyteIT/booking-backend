namespace Ube.Domain.Entities.Listings;

public class RestaurantListingDetails : IListingDetail
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string CuisineType { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public string? TableTypes { get; set; }
    public string? ReservationRules { get; set; }
}