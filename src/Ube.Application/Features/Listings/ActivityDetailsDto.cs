namespace Ube.Application.Features.Listings;

public class ActivityDetailsDto
{
    public string ActivityType { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public string? DifficultyLevel { get; set; }
    public decimal? Price { get; set; }
    public int? MinGroupSize { get; set; }
    public int? MaxGroupSize { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public List<string>? IncludedServices { get; set; }
    public string? SafetyRequirements { get; set; }
    public string? AvailabilitySchedule { get; set; }
}
