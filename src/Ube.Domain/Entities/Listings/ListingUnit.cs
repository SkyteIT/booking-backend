using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

// Generic bookable sub-inventory under a Listing - a hotel room type, a
// specific event seat, a car in a rental fleet, a restaurant time slot,
// etc. A Listing with zero units behaves exactly like before (booked as
// a whole, against Listing.Capacity) - units are opt-in per listing.
public class ListingUnit
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public ListingUnitKind Kind { get; set; } = ListingUnitKind.Generic;

    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal? PriceOverride { get; set; }
    public int Capacity { get; set; } = 1;

    // Seat kind only
    public int? RowIndex { get; set; }
    public int? ColumnIndex { get; set; }

    // TimeSlot kind only
    public TimeSpan? SlotStartTime { get; set; }
    public TimeSpan? SlotDuration { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
