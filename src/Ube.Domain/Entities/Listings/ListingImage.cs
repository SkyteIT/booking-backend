namespace Ube.Domain.Entities.Listings;

public class ListingImage
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;
}