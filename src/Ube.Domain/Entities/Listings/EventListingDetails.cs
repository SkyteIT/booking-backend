namespace Ube.Domain.Entities.Listings;

public class EventListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public DateTime EventDate { get; set; }
    public int DurationHours { get; set; }
    public decimal TicketPrice { get; set; }
    public string Organizer { get; set; } = string.Empty;

    public Listing Listing { get; set; } = default!;
}