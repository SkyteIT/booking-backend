namespace Ube.Application.Features.Listings.Commands;

public class ActivityDetailsDto
{
    public string ActivityType { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Location { get; set; } = string.Empty;
}