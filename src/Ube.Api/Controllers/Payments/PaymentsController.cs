using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IVendorAdvanceService _advanceService;
    private readonly IPaymentReconciliationService _reconciliationService;
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(
        IPaymentService paymentService,
        IVendorAdvanceService advanceService,
        IPaymentReconciliationService reconciliationService,
        ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _advanceService = advanceService;
        _reconciliationService = reconciliationService;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Initiate(InitiatePaymentRequest request, CancellationToken ct)
    {
        var result = await _paymentService.InitiateAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _paymentService.GetAsync(id, _currentUser.UserId, isAdmin, ct);
        return Ok(result);
    }

    [HttpPost("advance")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> IssueAdvance(IssueVendorAdvanceRequest request, CancellationToken ct)
    {
        var result = await _advanceService.IssueAdvanceAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpPost("reconcile")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Reconcile([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, CancellationToken ct)
    {
        var result = await _reconciliationService.ReconcileAsync(_currentUser.UserId, periodStart, periodEnd, ct);
        return Ok(result);
    }
}
