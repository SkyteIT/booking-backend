namespace Ube.Domain.Enums.Listings;

// Null on ListingOffer.DiscountType means a pure perk announcement (e.g.
// "Free airport pickup") with no price effect at all.
public enum OfferDiscountType
{
    PercentageDiscount = 1,
    FixedAmountDiscount = 2
}
