using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/payout-batches")]
public class PayoutBatchesController : ControllerBase
{
    private readonly IPayoutBatchService _payoutBatchService;
    private readonly ICurrentUserService _currentUser;

    public PayoutBatchesController(IPayoutBatchService payoutBatchService, ICurrentUserService currentUser)
    {
        _payoutBatchService = payoutBatchService;
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
}
