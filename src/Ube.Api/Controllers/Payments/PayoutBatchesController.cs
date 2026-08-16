using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize(Roles = "Admin,Finance")]
[Route("api/payout-batches")]
public class PayoutBatchesController : ControllerBase
{
    private readonly IPayoutBatchService _payoutBatchService;
    private readonly IPayoutExportService _payoutExportService;
    private readonly ICurrentUserService _currentUser;

    public PayoutBatchesController(
        IPayoutBatchService payoutBatchService,
        IPayoutExportService payoutExportService,
        ICurrentUserService currentUser)
    {
        _payoutBatchService = payoutBatchService;
        _payoutExportService = payoutExportService;
        _currentUser = currentUser;
    }

    [HttpPost("compute")]
    public async Task<IActionResult> Compute(ComputePayoutBatchRequest request, CancellationToken ct)
    {
        var result = await _payoutBatchService.ComputeAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/settle")]
    public async Task<IActionResult> Settle(Guid id, CancellationToken ct)
    {
        var result = await _payoutBatchService.SettleAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    // Maker: locks every Pending batch into one export run.
    [HttpPost("export/request")]
    public async Task<IActionResult> RequestExport(CancellationToken ct)
    {
        var result = await _payoutExportService.RequestExportAsync(_currentUser.UserId, ct);
        return Ok(result);
    }

    // Checker: must be a different admin than whoever requested it. If the
    // total is under the large-export threshold, this returns the actual
    // CSV file. If it's over the threshold, this only records the first
    // approval and returns JSON showing the run is now
    // PendingSeniorApproval - a THIRD admin must call export/{id}/senior-approve.
    [HttpPost("export/{id:guid}/approve")]
    public async Task<IActionResult> ApproveExport(Guid id, CancellationToken ct)
    {
        var result = await _payoutExportService.ApproveAsync(_currentUser.UserId, id, ct);
        return result.File != null
            ? File(result.File.Content, "text/csv", result.File.FileName)
            : Ok(result.Run);
    }

    // Only valid while the run is PendingSeniorApproval. Must be a THIRD
    // admin, different from both the requester and the first approver.
    [HttpPost("export/{id:guid}/senior-approve")]
    public async Task<IActionResult> GrantSeniorApproval(Guid id, CancellationToken ct)
    {
        var result = await _payoutExportService.GrantSeniorApprovalAsync(_currentUser.UserId, id, ct);
        return File(result.Content, "text/csv", result.FileName);
    }

    [HttpPost("export/{id:guid}/reject")]
    public async Task<IActionResult> RejectExport(Guid id, CancellationToken ct)
    {
        var result = await _payoutExportService.RejectAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpGet("export/threshold")]
    public async Task<IActionResult> GetExportThreshold(CancellationToken ct)
    {
        var result = await _payoutExportService.GetThresholdAsync(ct);
        return Ok(result);
    }

    [HttpPut("export/threshold")]
    public async Task<IActionResult> UpdateExportThreshold(UpdatePayoutExportThresholdRequest request, CancellationToken ct)
    {
        var result = await _payoutExportService.UpdateThresholdAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }
}
