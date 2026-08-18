using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

// A named date-range rate adjustment for a listing (or one specific unit
// of it) - "Peak Season: Dec 20 - Jan 5, +30%". Only meaningful for
// PerNight/PerDay categories, where a booking's price is naturally
// date-based; see BookingPricingRules.CalculateSeasonalTotal.
public class SeasonalPricingRule
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    // Null - applies to the listing's base Price. Set - applies only to
    // that specific unit's PriceOverride, never the listing's base price.
    public Guid? ListingUnitId { get; set; }
    public ListingUnit? ListingUnit { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public SeasonalRateAdjustmentType AdjustmentType { get; set; }
    public decimal AdjustmentValue { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
