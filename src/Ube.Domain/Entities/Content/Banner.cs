using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Domain.Entities.Content;

public class Banner
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public BannerPlacement Placement { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
