using Ube.Domain.Entities.Common;
using Ube.Domain.Enums;

namespace Ube.Domain.Entities.Content;

public class Promotion : BaseEntity
{
    public string PromoCode { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal Value { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}
