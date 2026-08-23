namespace Ube.Domain.Entities.Listings;

// A dimension the vendor defines on their own listing - "Room Type",
// "Ticket Tier", etc. Independent of ListingUnit/its Kind: applies
// whether the listing also uses Generic/Seat/TimeSlot units or none at
// all. A listing with zero groups behaves exactly as before - groups are
// opt-in, additive on top of whatever selection already exists.
public class ListingOptionGroup
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public ICollection<ListingOptionValue> Values { get; set; } = new List<ListingOptionValue>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
