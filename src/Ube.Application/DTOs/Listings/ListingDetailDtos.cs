namespace Ube.Application.DTOs.Listings;

public sealed class HotelDetailsDto
{
    public decimal PricePerNight { get; set; }
    public int AvailableRooms { get; set; }
    public List<string> Amenities { get; set; } = new();
    public string CheckInTime { get; set; } = string.Empty;
    public string CheckOutTime { get; set; } = string.Empty;
    public List<string> RoomTypes { get; set; } = new();
    public string? PropertyType { get; set; }
    public string? PrimaryRoomType { get; set; }
}

public sealed class RestaurantDetailsDto
{
    public string CuisineType { get; set; } = string.Empty;
    public decimal AverageCost { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public List<string> TableTypes { get; set; } = new();
    public string? ReservationRules { get; set; }
}

public sealed class CarRentalDetailsDto
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public int SeatCount { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
    public int? Year { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PickupLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public string? InsuranceOptions { get; set; }
}

public sealed class ActivityDetailsDto
{
    public string ActivityType { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MinGroupSize { get; set; }
    public int MaxGroupSize { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public List<string> IncludedServices { get; set; } = new();
    public string SafetyRequirements { get; set; } = string.Empty;
    public string AvailabilitySchedule { get; set; } = string.Empty;
}

public sealed class TicketTypeDto
{
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public sealed class EventDetailsDto
{
    public string EventName { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public string DateAndTime { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public decimal TicketPrice { get; set; }
    public string? EventType { get; set; }
    public string? VenueName { get; set; }
    public string? VenueAddress { get; set; }
    public List<TicketTypeDto>? TicketTypes { get; set; }
}