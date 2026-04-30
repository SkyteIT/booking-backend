using System.ComponentModel.DataAnnotations;
using Ube.Domain.Entities.Common;
using Ube.Domain.Enums;

namespace Ube.Domain.Entities.Content;

public class Banner : BaseEntity
{
    [MaxLength(int.MaxValue)]
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    [MaxLength(int.MaxValue)]
    public string ImageUrl { get; set; } = string.Empty;
    public BannerPlacement Placement { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}