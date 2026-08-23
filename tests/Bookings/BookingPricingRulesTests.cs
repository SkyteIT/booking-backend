using Ube.Application.Features.Bookings;
using Ube.Domain.Entities.Listings;
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
    public void CalculateTotal_FixedPrice_Ignores_Duration_And_Quantity()
    {
        var total = BookingPricingRules.CalculateTotal(2500m, 3, Start, Start.AddDays(10), PricingUnit.FixedPrice);
        Assert.Equal(2500m, total); // flat price per booking - quantity is party size, not a multiplier
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

    private static SeasonalPricingRule MakeRule(DateTime start, DateTime end, SeasonalRateAdjustmentType type, decimal value) => new()
    {
        Id = Guid.NewGuid(),
        StartDate = DateOnly.FromDateTime(start),
        EndDate = DateOnly.FromDateTime(end),
        AdjustmentType = type,
        AdjustmentValue = value,
        IsActive = true
    };

    [Fact]
    public void CalculateSeasonalTotal_With_No_Rules_Matches_Flat_CalculateTotal()
    {
        var seasonal = BookingPricingRules.CalculateSeasonalTotal(1000m, 2, Start, Start.AddDays(3), Array.Empty<SeasonalPricingRule>());
        var flat = BookingPricingRules.CalculateTotal(1000m, 2, Start, Start.AddDays(3), PricingUnit.PerNight);
        Assert.Equal(flat, seasonal);
    }

    [Fact]
    public void CalculateSeasonalTotal_Rule_Covering_Whole_Stay_Applies_Percentage_Discount()
    {
        var rules = new[] { MakeRule(Start, Start.AddDays(10), SeasonalRateAdjustmentType.PercentageAdjustment, -20) };
        var total = BookingPricingRules.CalculateSeasonalTotal(1000m, 1, Start, Start.AddDays(3), rules);
        Assert.Equal(2400m, total); // 3 nights * 800 (1000 - 20%)
    }

    [Fact]
    public void CalculateSeasonalTotal_Rule_Covering_Only_Part_Of_Stay_Sums_Mixed_Rates()
    {
        // 4-night stay, only the last 2 nights fall in a +50% peak rule.
        var peakStart = Start.AddDays(2);
        var rules = new[] { MakeRule(peakStart, Start.AddDays(10), SeasonalRateAdjustmentType.PercentageAdjustment, 50) };
        var total = BookingPricingRules.CalculateSeasonalTotal(1000m, 1, Start, Start.AddDays(4), rules);
        // nights: day0=1000, day1=1000, day2=1500, day3=1500 => 5000
        Assert.Equal(5000m, total);
    }

    [Fact]
    public void CalculateSeasonalTotal_FixedRate_Replaces_Base_Price_For_Covered_Nights()
    {
        var rules = new[] { MakeRule(Start, Start, SeasonalRateAdjustmentType.FixedRate, 750m) };
        var total = BookingPricingRules.CalculateSeasonalTotal(1000m, 2, Start, Start.AddDays(2), rules);
        // night0 fixed 750, night1 falls outside the rule -> base 1000; *2 quantity
        Assert.Equal((750m + 1000m) * 2, total);
    }

    [Fact]
    public void CalculateSeasonalTotal_Inactive_Rule_Is_Ignored()
    {
        var rule = MakeRule(Start, Start.AddDays(10), SeasonalRateAdjustmentType.FixedRate, 1);
        rule.IsActive = false;
        var total = BookingPricingRules.CalculateSeasonalTotal(1000m, 1, Start, Start.AddDays(2), new[] { rule });
        Assert.Equal(2000m, total); // falls back to flat base price
    }

    private static ListingOffer MakeOffer(OfferDiscountType? type, decimal? value) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Test Offer",
        StartDate = DateOnly.FromDateTime(Start),
        EndDate = DateOnly.FromDateTime(Start.AddDays(30)),
        DiscountType = type,
        DiscountValue = value,
        IsActive = true
    };

    [Fact]
    public void ApplyOfferDiscount_Returns_Total_Unchanged_When_Offer_Is_Null()
    {
        Assert.Equal(1000m, BookingPricingRules.ApplyOfferDiscount(1000m, null));
    }

    [Fact]
    public void ApplyOfferDiscount_Returns_Total_Unchanged_For_Pure_Perk_Offer()
    {
        var offer = MakeOffer(null, null);
        Assert.Equal(1000m, BookingPricingRules.ApplyOfferDiscount(1000m, offer));
    }

    [Fact]
    public void ApplyOfferDiscount_Applies_Percentage_Discount_Once()
    {
        var offer = MakeOffer(OfferDiscountType.PercentageDiscount, 20);
        Assert.Equal(800m, BookingPricingRules.ApplyOfferDiscount(1000m, offer));
    }

    [Fact]
    public void ApplyOfferDiscount_Applies_Fixed_Discount_And_Never_Goes_Negative()
    {
        var offer = MakeOffer(OfferDiscountType.FixedAmountDiscount, 1500);
        Assert.Equal(0m, BookingPricingRules.ApplyOfferDiscount(1000m, offer));
    }
}
