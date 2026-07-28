using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize(Roles = "Admin,Finance")]
[Route("api/payment-disputes")]
public class PaymentDisputesController : ControllerBase
{
    private readonly IPaymentDisputeService _disputeService;
    private readonly ICurrentUserService _currentUser;

    public PaymentDisputesController(IPaymentDisputeService disputeService, ICurrentUserService currentUser)
    {
        _disputeService = disputeService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> RecordDispute(RecordDisputeRequest request, CancellationToken ct)
    {
        var result = await _disputeService.RecordDisputeAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> ResolveDispute(Guid id, ResolveDisputeRequest request, CancellationToken ct)
    {
        var result = await _disputeService.ResolveDisputeAsync(_currentUser.UserId, id, request, ct);
        return Ok(result);
    }

    [HttpGet("payment/{paymentId:guid}")]
    public async Task<IActionResult> GetForPayment(Guid paymentId, CancellationToken ct)
    {
        var result = await _disputeService.GetForPaymentAsync(paymentId, ct);
        return Ok(result);
    }
}
