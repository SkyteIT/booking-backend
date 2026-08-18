using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/role-change-requests")]
public class RoleChangeRequestsController : ControllerBase
{
    private readonly IRoleChangeRequestService _requestService;
    private readonly ICurrentUserService _currentUser;

    public RoleChangeRequestsController(IRoleChangeRequestService requestService, ICurrentUserService currentUser)
    {
        _requestService = requestService;
        _currentUser = currentUser;
    }

    // The approval queue - only SuperAdmin ever gets to see every
    // pending request, matching "watch everything, approve changes."
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] RoleChangeRequestStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _requestService.GetPagedAsync(status, pageNumber, pageSize, ct);
        return Ok(result);
    }

    // Lets a requesting Admin see their own asks' status, without
    // exposing anyone else's.
    [HttpGet("mine")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await _requestService.GetMineAsync(_currentUser.UserId, ct);
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
    public async Task<IActionResult> Reject(Guid id, RejectRoleChangeRequestRequest request, CancellationToken ct)
    {
        var result = await _requestService.RejectAsync(id, _currentUser.UserId, request.Notes, ct);
        return Ok(result);
    }
}
