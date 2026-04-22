namespace Ube.Domain.Entities.Listings;

public class HotelListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public decimal PricePerNight { get; set; }
    public string Location { get; set; } = string.Empty;
    public int AvailableRooms { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }

    public Listing Listing { get; set; } = default!;
}