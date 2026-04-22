namespace Ube.Application.Features.Listings.Commands;

public class HotelDetailsDto
{
    public decimal PricePerNight { get; set; }
    public string Location { get; set; } = string.Empty;
    public int AvailableRooms { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
}