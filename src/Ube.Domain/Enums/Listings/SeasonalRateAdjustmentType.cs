namespace Ube.Domain.Enums.Listings;

public enum SeasonalRateAdjustmentType
{
    // AdjustmentValue is a percent applied to the base price - negative
    // for a discount (e.g. -15), positive for a markup (e.g. 30).
    PercentageAdjustment = 1,

    // AdjustmentValue replaces the base price outright for covered dates.
    FixedRate = 2
}
