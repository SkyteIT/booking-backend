using Ube.Application.Features.Bookings;
using Ube.Domain.Enums.Listings;

namespace Ube.Tests.Bookings;

public class BookingPricingRulesTests
{
    private static readonly DateTime Start = new(2026, 1, 1);

    [Fact]
    public void CalculateTotal_Defaults_To_Per_Day_When_No_PricingUnit_Given()
    {
        var total = BookingPricingRules.CalculateTotal(1000m, 2, Start, Start.AddDays(3));
        Assert.Equal(6000m, total); // 1000 * 2 qty * 3 days
    }

    [Fact]
    public void CalculateTotal_PerNight_Multiplies_By_Days()
    {
        var total = BookingPricingRules.CalculateTotal(1000m, 1, Start, Start.AddDays(4), PricingUnit.PerNight);
        Assert.Equal(4000m, total);
    }

    [Fact]
    public void CalculateTotal_PerDay_Multiplies_By_Days()
    {
        var total = BookingPricingRules.CalculateTotal(500m, 2, Start, Start.AddDays(2), PricingUnit.PerDay);
        Assert.Equal(2000m, total); // 500 * 2 qty * 2 days
    }

    [Fact]
    public void CalculateTotal_FixedPrice_Ignores_Duration()
    {
        var total = BookingPricingRules.CalculateTotal(2500m, 3, Start, Start.AddDays(10), PricingUnit.FixedPrice);
        Assert.Equal(7500m, total); // 2500 * 3 qty, duration irrelevant
    }

    [Fact]
    public void CalculateTotal_PerPerson_Ignores_Duration()
    {
        var total = BookingPricingRules.CalculateTotal(1500m, 4, Start, Start.AddDays(1), PricingUnit.PerPerson);
        Assert.Equal(6000m, total); // 1500 * 4 guests
    }

    [Fact]
    public void CalculateTotal_PerHour_Multiplies_By_Hours()
    {
        var total = BookingPricingRules.CalculateTotal(200m, 1, Start, Start.AddHours(3), PricingUnit.PerHour);
        Assert.Equal(600m, total);
    }

    [Fact]
    public void CalculateTotal_Same_Day_Booking_Uses_Minimum_One_Day()
    {
        var total = BookingPricingRules.CalculateTotal(1000m, 1, Start, Start.AddHours(2), PricingUnit.PerDay);
        Assert.Equal(1000m, total); // same calendar day -> at least 1 day
    }
}
