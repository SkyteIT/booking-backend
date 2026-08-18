using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Listings;

public class ListingOfferDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OfferDiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateListingOfferRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OfferDiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class UpdateListingOfferRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public OfferDiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool? IsActive { get; set; }
    // Explicit flag needed since DiscountType/DiscountValue being null in
    // the request could mean "leave unchanged" or "clear to a pure perk" -
    // set true to mean the latter.
    public bool ClearDiscount { get; set; }
}
