namespace Ube.Domain.Enums.Payments;

public enum PayoutExportStatus
{
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,

    // Total exceeds the configured large-export threshold: the first
    // approver's check passed, but a second, distinct admin must sign off
    // before the file is actually generated.
    PendingSeniorApproval = 4
}
