using System.ComponentModel.DataAnnotations;

namespace Ube.Application.DTOs.Promotion;

public class CreatePromotionDto
{
    // FIX: renamed from "PromoCode" to "Code" to match frontend payload field { code: ... }
    [Required(ErrorMessage = "Promo code is required.")]
    [MinLength(1, ErrorMessage = "Promo code cannot be empty.")]
    public string Code { get; set; } = string.Empty;

    // FIX: renamed from "Type" to "PromotionType" to match frontend payload field { promotionType: ... }
    public int PromotionType { get; set; }   // 0 = Percentage, 1 = Fixed Amount

    // FIX: renamed from "Value" to "DiscountValue" to match frontend payload field { discountValue: ... }
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than zero.")]
    public decimal DiscountValue { get; set; }

    public int? UsageLimit { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}