namespace Ube.Application.Features.Listings.Commands;

public class EventDetailsDto
{
    public string EventName { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public DateTime DateAndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public decimal TicketPrice { get; set; }
}