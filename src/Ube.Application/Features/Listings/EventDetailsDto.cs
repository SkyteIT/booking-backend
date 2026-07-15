namespace Ube.Application.Features.Listings;

public class EventDetailsDto
{
    public string EventName { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public DateTime DateAndTime { get; set; }
    public int SeatCount { get; set; }
    public decimal TicketPrice { get; set; }
    public string? EventType { get; set; }
    public string? VenueName { get; set; }
    public string? VenueAddress { get; set; }
    public List<TicketTypeDto>? TicketTypes { get; set; }
}

public class TicketTypeDto
{
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
