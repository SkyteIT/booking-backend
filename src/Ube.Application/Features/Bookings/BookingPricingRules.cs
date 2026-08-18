using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Bookings;

public static class BookingPricingRules
{
    // effectivePrice is resolved by the caller as unit?.PriceOverride ??
    // listing.Price before calling in - this stays unit-agnostic.

    public static decimal CalculateTotal(
        decimal effectivePrice,
        int quantity,
        DateTime start,
        DateTime end,
        PricingUnit? pricingUnit = null)
    {
        var days = Math.Max(1, (end.Date - start.Date).Days);

        return pricingUnit switch
        {
            PricingUnit.FixedPrice => effectivePrice * quantity,
            PricingUnit.PerPerson => effectivePrice * quantity,
            PricingUnit.PerHour => effectivePrice * quantity * Math.Max(1, (decimal)(end - start).TotalHours),
            PricingUnit.PerNight or PricingUnit.PerDay or null => effectivePrice * quantity * days,
            _ => effectivePrice * quantity * days
        };
    }

    // Date-aware version for PerNight/PerDay only - sums each night's own
    // rate (base price, adjusted by whichever seasonal rule covers that
    // night, if any) instead of a flat multiply. With an empty rules list
    // this produces exactly the same total as CalculateTotal for
    // PerNight/PerDay, so listings with no seasonal rules see zero
    // behavior change. Rules are assumed non-overlapping (enforced at
    // write time), so at most one rule ever matches a given night.
    public static decimal CalculateSeasonalTotal(
        decimal basePrice,
        int quantity,
        DateTime start,
        DateTime end,
        IReadOnlyList<SeasonalPricingRule> rules)
    {
        var days = Math.Max(1, (end.Date - start.Date).Days);
        decimal nightsTotal = 0;

        for (var i = 0; i < days; i++)
        {
            var night = DateOnly.FromDateTime(start.Date.AddDays(i));
            var rule = rules.FirstOrDefault(r => r.IsActive && r.StartDate <= night && r.EndDate >= night);

            nightsTotal += rule switch
            {
                { AdjustmentType: SeasonalRateAdjustmentType.FixedRate } => rule.AdjustmentValue,
                { AdjustmentType: SeasonalRateAdjustmentType.PercentageAdjustment } => basePrice * (1 + rule.AdjustmentValue / 100m),
                _ => basePrice
            };
        }

        return nightsTotal * quantity;
    }
}
