namespace Ube.Domain.Entities.Listings;

public class ActivityListingDetails
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }

    public string ActivityType { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Location { get; set; } = string.Empty;

    public Listing Listing { get; set; } = default!;
}