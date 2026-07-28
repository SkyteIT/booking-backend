namespace Ube.Domain.Entities.Payments;

// Singleton settings row (exactly one exists) - the dollar amount above
// which a PayoutExportRun needs a second, distinct admin's sign-off
// before the file is generated. Admin-editable, not hardcoded.
public class PayoutExportSettings
{
    public Guid Id { get; set; }
    public decimal LargeExportThreshold { get; set; } = 5000m;

    public Guid? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
