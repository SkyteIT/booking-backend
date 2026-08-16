namespace Ube.Domain.Entities.Payments;

// Proves the vendor was shown the rate that applied to them at a given
// point in time - required for Platform-to-Business-style disclosure
// obligations, not just a UI checkbox that's easy to lose track of.
public class VendorCommissionAcknowledgement
{
    public Guid Id { get; set; }

    public Guid VendorProfileId { get; set; }
    public decimal CommissionPercentShown { get; set; }
    public Guid? SourceOverrideId { get; set; }

    public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
}
