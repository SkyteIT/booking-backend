namespace Ube.Application.Features.Content.Promotion;

public class PromotionDto
{
    public Guid Id { get; set; }

    // FIX: renamed to "code" so frontend can read p.code correctly
    public string Code { get; set; } = string.Empty;

    // FIX: renamed to "promotionType" as int (0=Percentage, 1=Fixed) so
    //      frontend mapping (p.promotionType === 0 ? "Percentage" : "Fixed Amount") works
    public int PromotionType { get; set; }

    // FIX: renamed to "discountValue" so frontend can read p.discountValue correctly
    public decimal DiscountValue { get; set; }

    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    // FIX: renamed to "isActive" (bool) so frontend status logic works:
    //      derivePromotionStatus(startDate, endDate, p.isActive)
    public bool IsActive { get; set; }
}