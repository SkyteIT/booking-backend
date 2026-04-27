namespace Ube.Domain.Entities.Listings;

public class HotelListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public decimal PricePerNight { get; set; }
    public int AvailableRooms { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public string RoomTypes { get; set; } = string.Empty;
    public string CheckInTime { get; set; } = string.Empty;
    public string CheckOutTime { get; set; } = string.Empty;
    public string? PropertyType { get; set; }
    public string? PrimaryRoomType { get; set; }
    public string? Images { get; set; }

    public Listing Listing { get; set; } = default!;
}