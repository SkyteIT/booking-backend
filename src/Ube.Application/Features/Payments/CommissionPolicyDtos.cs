using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class CreateCommissionOverrideRequest
{
    public Guid VendorProfileId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal CommissionPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class VendorCommissionOverrideDto
{
    public Guid Id { get; set; }
    public Guid VendorProfileId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal CommissionPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public CommissionOverrideStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? RevokedByUserId { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class CreateLoyaltyTierRequest
{
    public int MonthsActive { get; set; }
    public decimal DiscountPercent { get; set; }
}

public class UpdateLoyaltyTierRequest
{
    public decimal DiscountPercent { get; set; }
}

public class LoyaltyDiscountTierDto
{
    public Guid Id { get; set; }
    public int MonthsActive { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
