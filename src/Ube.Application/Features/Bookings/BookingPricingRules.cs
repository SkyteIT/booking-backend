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
            // A fixed price is per booking, not per person/unit - a table
            // for 2 costs the same whether 1 or 2 people actually show up;
            // quantity there is party size for capacity checks, not a
            // price multiplier. PerPerson is the pricing unit for "each
            // person pays their own way" listings instead.
            PricingUnit.FixedPrice => effectivePrice,
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

    // Applies at most one offer's discount to an already-computed total
    // (post seasonal pricing) - once per booking, not per night, since an
    // offer is a marketing deal, not a rate structure. A null offer or an
    // offer with no DiscountType (a pure perk) leaves the total untouched.
    // Shared by CheckoutService and the price-quote endpoint so the two
    // can never disagree about what a customer is actually charged.
    public static decimal ApplyOfferDiscount(decimal total, ListingOffer? offer)
    {
        if (offer?.DiscountType is null || !offer.DiscountValue.HasValue)
            return total;

        var discounted = offer.DiscountType switch
        {
            OfferDiscountType.PercentageDiscount => total * (1 - offer.DiscountValue.Value / 100m),
            OfferDiscountType.FixedAmountDiscount => total - offer.DiscountValue.Value,
            _ => total
        };

        return Math.Max(0, discounted);
    }
}
