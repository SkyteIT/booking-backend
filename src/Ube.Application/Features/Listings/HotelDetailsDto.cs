namespace Ube.Application.Features.Listings;

public class HotelDetailsDto
{
    public decimal PricePerNight { get; set; }
    public int AvailableRooms { get; set; }
    public List<string> Amenities { get; set; } = new();
    public List<string> RoomTypes { get; set; } = new();
    public string CheckInTime { get; set; } = string.Empty;
    public string CheckOutTime { get; set; } = string.Empty;
    public string? PropertyType { get; set; }
    public string? PrimaryRoomType { get; set; }
}
