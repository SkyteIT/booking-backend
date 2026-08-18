using Ube.Domain.Enums.Listings;

namespace Ube.Domain.Entities.Listings;

// A vendor-written, customer-facing offer for a listing - a title +
// description (e.g. "Free airport pickup with every booking"), with an
// OPTIONAL discount attached. Deliberately not only a price mechanism -
// DiscountType/DiscountValue are both null for a pure perk offer.
// StartDate/EndDate are a book-by window ("book before Sep 1"), checked
// against today at checkout time - independent of the customer's stay
// dates, matching how real marketplace "special offers" behave.
public class ListingOffer
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public OfferDiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
