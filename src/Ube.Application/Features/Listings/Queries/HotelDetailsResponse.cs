namespace Ube.Application.Features.Listings.Queries;

public class HotelDetailsResponse
{
    public string? PropertyType { get; set; }

    public int NumberOfRooms { get; set; }

    public TimeSpan? CheckInTime { get; set; }

    public TimeSpan? CheckOutTime { get; set; }

    public string RoomTypes { get; set; } = string.Empty;

    public string Amenities { get; set; } = string.Empty;

    public string? CancellationPolicy { get; set; }
}