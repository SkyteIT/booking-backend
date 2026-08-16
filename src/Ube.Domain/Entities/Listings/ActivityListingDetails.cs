namespace Ube.Domain.Entities.Listings;

public class ActivityListingDetails : IListingDetail
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = null!;

    public string ActivityType { get; set; } = string.Empty;

    public int DurationHours { get; set; }

    public string? DifficultyLevel { get; set; }

    public decimal? Price { get; set; }

    public int? MinGroupSize { get; set; }
    public int? MaxGroupSize { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public string? IncludedServices { get; set; } // comma separated
    public string? SafetyRequirements { get; set; }
    public string? AvailabilitySchedule { get; set; }
}