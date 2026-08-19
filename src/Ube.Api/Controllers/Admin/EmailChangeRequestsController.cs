using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/email-change-requests")]
public class EmailChangeRequestsController : ControllerBase
{
    private readonly IEmailChangeRequestService _requestService;
    private readonly ICurrentUserService _currentUser;

    public EmailChangeRequestsController(IEmailChangeRequestService requestService, ICurrentUserService currentUser)
    {
        _requestService = requestService;
        _currentUser = currentUser;
    }

    // Any authenticated account can ask to change its own email - never
    // applied directly, always queued for a SuperAdmin to review.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailChangeRequestDto dto, CancellationToken ct)
    {
        var result = await _requestService.CreateAsync(_currentUser.UserId, dto.RequestedEmail, dto.Reason, ct);
        return Ok(result);
    }

    // Lets the requester see their own asks' status, without exposing
    // anyone else's.
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await _requestService.GetMineAsync(_currentUser.UserId, ct);
        return Ok(result);
    }

    // The approval queue - only SuperAdmin ever sees every pending
    // request, matching "watch everything, approve changes."
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] EmailChangeRequestStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _requestService.GetPagedAsync(status, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _requestService.ApproveAsync(id, _currentUser.UserId, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Reject(Guid id, RejectEmailChangeRequestRequest request, CancellationToken ct)
    {
        var result = await _requestService.RejectAsync(id, _currentUser.UserId, request.Notes, ct);
        return Ok(result);
    }
}
