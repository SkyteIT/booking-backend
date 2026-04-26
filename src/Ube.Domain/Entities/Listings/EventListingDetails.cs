namespace Ube.Domain.Entities.Listings;

public class EventListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public string EventName { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public DateTime DateAndTime { get; set; }
    public int SeatCount { get; set; }
    public decimal TicketPrice { get; set; }

    public Listing Listing { get; set; } = default!;
}