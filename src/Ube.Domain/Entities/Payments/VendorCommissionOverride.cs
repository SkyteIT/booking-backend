using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

// Highest-priority layer in commission resolution. Editable only before
// StartDate; once live, a rate change means revoking this and creating a
// new one - so there's never ambiguity about what rate applied on a given
// day.
public class VendorCommissionOverride
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }
    public Guid? CategoryId { get; set; }

    public decimal CommissionPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }
    public CommissionOverrideStatus Status { get; set; } = CommissionOverrideStatus.Active;

    public Guid? RevokedByUserId { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
