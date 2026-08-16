namespace Ube.Domain.Entities.Payments;

// Small admin-editable table, e.g. {12, 1%}, {24, 2%}. The resolver takes
// the highest tier a vendor's tenure (VendorProfile.CreatedAt) qualifies
// for. Applies only when the vendor has no active VendorCommissionOverride.
public class LoyaltyDiscountTier
{
    public Guid Id { get; set; }

    public int MonthsActive { get; set; }
    public decimal DiscountPercent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
