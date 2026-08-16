using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize]
[Route("api/refunds")]
public class RefundsController : ControllerBase
{
    private readonly IRefundService _refundService;
    private readonly ICurrentUserService _currentUser;

    public RefundsController(IRefundService refundService, ICurrentUserService currentUser)
    {
        _refundService = refundService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund(RequestRefundRequest request, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _refundService.RequestAsync(_currentUser.UserId, isAdmin, request, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _refundService.ApproveAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        var result = await _refundService.RejectAsync(_currentUser.UserId, id, reason, ct);
        return Ok(result);
    }
}
