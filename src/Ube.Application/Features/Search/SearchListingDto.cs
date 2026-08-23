using Ube.Domain.Enums.Listings;

namespace Ube.Application.Features.Search;

public class SearchListingDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "LKR";
    public double AverageRating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool HasActiveOffer { get; set; }
    public string? OfferBadgeText { get; set; }
}
