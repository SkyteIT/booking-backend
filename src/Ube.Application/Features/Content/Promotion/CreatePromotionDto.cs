namespace Ube.Application.Features.Content.Promotion;

public class CreatePromotionDto
{
    public string PromoCode { get; set; } = string.Empty;
    public int Type { get; set; }
    public decimal Value { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
