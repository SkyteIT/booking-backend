namespace Ube.Domain.Entities.Listings;

public class BlockedDate
{
    public Guid Id {get; set;}
    public Guid ListingId { get; set; }
    public DateTime Date {get; set;}

    public DateTime CreatedAt {get; set;}
    
}