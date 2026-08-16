using Ube.Domain.Enums.Payments;

namespace Ube.Application.Features.Payments;

public class PayoutExportRunDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<Guid> BatchIds { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public PayoutExportStatus Status { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? SecondApprovedByUserId { get; set; }
    public DateTime? SecondApprovedAt { get; set; }
    public string? FileChecksum { get; set; }
}

public class PayoutExportFileResult
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string Checksum { get; set; } = string.Empty;
}

// The first approval either finishes the job (File is set) or, if the
// total is over the large-export threshold, just moves the run into
// PendingSeniorApproval (File is null) - the caller must check which.
public class PayoutExportApprovalResult
{
    public PayoutExportRunDto Run { get; set; } = new();
    public PayoutExportFileResult? File { get; set; }
}

public class UpdatePayoutExportThresholdRequest
{
    public decimal LargeExportThreshold { get; set; }
}

public class PayoutExportThresholdDto
{
    public decimal LargeExportThreshold { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
