namespace Ube.Application.Features.Payments;

public interface IPayoutExportService
{
    // Maker: locks every currently-Pending batch into this export run.
    Task<PayoutExportRunDto> RequestExportAsync(Guid requestedByUserId, CancellationToken ct = default);

    // Any admin needs to discover a run someone else started, not just
    // the browser session that requested it.
    Task<IReadOnlyList<PayoutExportRunDto>> GetPendingAsync(CancellationToken ct = default);

    // Checker: must be a different admin than the requester. If the run's
    // total is under the configured threshold, this generates and returns
    // the file immediately (Result.File is set). If it's over the
    // threshold, this only records the first approval and moves the run
    // to PendingSeniorApproval - Result.File is null, and
    // GrantSeniorApprovalAsync must be called next by a THIRD admin.
    Task<PayoutExportApprovalResult> ApproveAsync(Guid approvedByUserId, Guid exportRunId, CancellationToken ct = default);

    // Only valid when the run is PendingSeniorApproval. The senior
    // approver must differ from both the requester and the first
    // approver. Always generates and returns the file.
    Task<PayoutExportFileResult> GrantSeniorApprovalAsync(Guid seniorApproverUserId, Guid exportRunId, CancellationToken ct = default);

    // Checker declines it - the locked batches go back to Pending so they
    // can be included in a future export instead.
    Task<PayoutExportRunDto> RejectAsync(Guid actorUserId, Guid exportRunId, CancellationToken ct = default);

    Task<PayoutExportThresholdDto> GetThresholdAsync(CancellationToken ct = default);
    Task<PayoutExportThresholdDto> UpdateThresholdAsync(Guid actorUserId, UpdatePayoutExportThresholdRequest request, CancellationToken ct = default);
}
