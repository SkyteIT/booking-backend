using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Domain.Entities.Content;

public class Promotion
{
    public Guid Id { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal Value { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
