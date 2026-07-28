using Ube.Domain.Enums.Payments;

namespace Ube.Domain.Entities.Payments;

// Maker-checker on the export itself: RequestedByUserId "prepares" the run
// (locking the chosen batches into Exported), a DIFFERENT admin must
// approve before the actual file is generated. FileChecksum is recorded
// at approval time so the exact contents that were downloaded can be
// verified later if a bank transfer is ever disputed.
public class PayoutExportRun
{
    public Guid Id { get; set; }

    public string BatchIdsJson { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    public PayoutExportStatus Status { get; set; } = PayoutExportStatus.PendingApproval;

    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Only set when TotalAmount exceeded the large-export threshold and a
    // second, distinct admin had to sign off before the file was generated.
    public Guid? SecondApprovedByUserId { get; set; }
    public DateTime? SecondApprovedAt { get; set; }

    public string? FileChecksum { get; set; }
}
