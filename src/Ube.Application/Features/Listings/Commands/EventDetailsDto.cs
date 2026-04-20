namespace Ube.Application.Features.Listings.Commands;

public class EventDetailsDto
{
    public DateTime EventDate { get; set; }
    public int DurationHours { get; set; }
    public decimal TicketPrice { get; set; }
    public string Organizer { get; set; } = string.Empty;
}