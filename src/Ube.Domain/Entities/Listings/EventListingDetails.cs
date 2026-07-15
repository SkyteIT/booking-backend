namespace Ube.Domain.Entities.Listings;

public class EventListingDetails : IListingDetail
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string EventName { get; set; } = string.Empty;
    public string Organizer { get; set; } = string.Empty;
    public DateTime DateAndTime { get; set; }
    public int SeatCount { get; set; }
    public decimal TicketPrice { get; set; }
    public string? EventType { get; set; }
    public string? VenueName { get; set; }
    public string? VenueAddress { get; set; }
    public string? TicketTypesJson { get; set; }
}