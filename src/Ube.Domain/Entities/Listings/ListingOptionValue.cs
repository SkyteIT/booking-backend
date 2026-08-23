using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

public class ListingOptionValue
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }
    public ListingOptionGroup Group { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public decimal PriceModifier { get; set; }
    // When set, replaces the base rate (unit.PriceOverride ?? listing.Price)
    // entirely instead of adding to it - e.g. a "Family package" that's a
    // different per-person rate, not an adjustment to the base one. Still
    // flows through the same quantity/date multiplication as any other
    // base price. Null = no override, PriceModifier applies as usual.
    public decimal? PriceOverride { get; set; }
    public BookingConfirmationType? ConfirmationTypeOverride { get; set; }

    public bool RequiresSeatSelection { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
