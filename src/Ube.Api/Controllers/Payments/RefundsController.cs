using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;
using Ube.Domain.Enums.Payments;

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

    // Admin queue view - lists refunds across the platform, optionally
    // filtered by status. Distinct from the payment-scoped/single-record
    // actions below, which any authenticated user can hit for their own booking.
    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> GetAll(
        [FromQuery] RefundStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _refundService.GetPagedAsync(status, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund(RequestRefundRequest request, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _refundService.RequestAsync(_currentUser.UserId, isAdmin, request, ct);
        return Ok(result);
    }

    // Admin can see the refund queue (customer support needs the
    // context), but only Finance actually moves money - approving/
    // rejecting is deliberately Finance-only, not Admin,Finance.
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _refundService.ApproveAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        var result = await _refundService.RejectAsync(_currentUser.UserId, id, reason, ct);
        return Ok(result);
    }
}
