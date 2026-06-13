using System.ComponentModel.DataAnnotations;

namespace Ube.Application.DTOs.Promotion;

public class UpdatePromotionDto
{
    // FIX: renamed from "PromoCode" to "Code" to match frontend payload { code: payload.promoCode }
    [Required(ErrorMessage = "Promo code is required.")]
    public string Code { get; set; } = string.Empty;

    // FIX: renamed from "Type" to "PromotionType" to match frontend payload { promotionType: payload.type }
    public int PromotionType { get; set; }   // 0 = Percentage, 1 = Fixed Amount

    // FIX: renamed from "Value" to "DiscountValue" to match frontend payload { discountValue: payload.value }
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than zero.")]
    public decimal DiscountValue { get; set; }

    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    // FIX: renamed from "Status" (int) to "IsActive" (bool) to match frontend payload { isActive: bool }
    public bool IsActive { get; set; }
}