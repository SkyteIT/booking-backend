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
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
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
}
