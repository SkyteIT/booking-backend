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
}
