using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Fraud;
using Ube.Domain.Enums.Fraud;

namespace Ube.Api.Controllers.Fraud;

[ApiController]
[Authorize(Roles = "Admin,Finance")]
[Route("api/admin/fraud-flags")]
public class FraudFlagsController : ControllerBase
{
    private readonly IFraudDetectionService _fraudService;
    private readonly ICurrentUserService _currentUser;

    public FraudFlagsController(IFraudDetectionService fraudService, ICurrentUserService currentUser)
    {
        _fraudService = fraudService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] FraudFlagStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _fraudService.GetPagedAsync(status, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, ReviewFraudFlagRequest request, CancellationToken ct)
    {
        var result = await _fraudService.ReviewAsync(_currentUser.UserId, id, request, ct);
        return Ok(result);
    }
}
