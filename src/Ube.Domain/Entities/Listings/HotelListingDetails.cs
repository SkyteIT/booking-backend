namespace Ube.Domain.Entities.Listings;

public class HotelListingDetails : IListingDetail
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public decimal PricePerNight { get; set; }
    public int AvailableRooms { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public string RoomTypes { get; set; } = string.Empty;
    public string CheckInTime { get; set; } = string.Empty;
    public string CheckOutTime { get; set; } = string.Empty;
    public string? PropertyType { get; set; }
    public string? PrimaryRoomType { get; set; }

    // Guests included in PricePerNight before occupancy pricing kicks in -
    // matches Booking.com's "double occupancy" base concept.
    public int BaseOccupancy { get; set; } = 2;
    // Currency amount added per night for each guest beyond BaseOccupancy.
    // Null = no occupancy pricing (today's flat-per-room behavior).
    public decimal? OccupancyPriceModifier { get; set; }
}