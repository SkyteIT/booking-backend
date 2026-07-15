namespace Ube.Application.Features.Content.Promotion;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
