namespace Ube.Domain.Enums.Listings;

// What Listing.Price is denominated in for this category - drives
// BookingPricingRules.CalculateTotal's multiplier. Matches the existing
// admin category-form options (was previously free text: "Per Night",
// "Per Hour", "Per Person", "Per Day", "Fixed Price").
public enum PricingUnit
{
    PerNight = 1,
    PerHour = 2,
    PerPerson = 3,
    PerDay = 4,
    FixedPrice = 5
}
