using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/commission-policy")]
public class CommissionPolicyController : ControllerBase
{
    private readonly ICommissionPolicyService _service;
    private readonly IVendorCommissionAcknowledgementService _acknowledgementService;
    private readonly ICurrentUserService _currentUser;

    public CommissionPolicyController(
        ICommissionPolicyService service,
        IVendorCommissionAcknowledgementService acknowledgementService,
        ICurrentUserService currentUser)
    {
        _service = service;
        _acknowledgementService = acknowledgementService;
        _currentUser = currentUser;
    }

    [HttpGet("acknowledgements/vendor/{vendorProfileId:guid}")]
    public async Task<IActionResult> GetAcknowledgementsForVendor(Guid vendorProfileId, CancellationToken ct)
    {
        var result = await _acknowledgementService.GetHistoryAsync(vendorProfileId, ct);
        return Ok(result);
    }

    [HttpPost("overrides")]
    public async Task<IActionResult> CreateOverride(CreateCommissionOverrideRequest request, CancellationToken ct)
    {
        var result = await _service.CreateOverrideAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpPost("overrides/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeOverride(Guid id, CancellationToken ct)
    {
        var result = await _service.RevokeOverrideAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpGet("overrides/vendor/{vendorProfileId:guid}")]
    public async Task<IActionResult> GetOverridesForVendor(Guid vendorProfileId, CancellationToken ct)
    {
        var result = await _service.GetOverridesForVendorAsync(vendorProfileId, ct);
        return Ok(result);
    }

    [HttpGet("loyalty-tiers")]
    public async Task<IActionResult> GetLoyaltyTiers(CancellationToken ct)
    {
        var result = await _service.GetLoyaltyTiersAsync(ct);
        return Ok(result);
    }

    [HttpPost("loyalty-tiers")]
    public async Task<IActionResult> CreateLoyaltyTier(CreateLoyaltyTierRequest request, CancellationToken ct)
    {
        var result = await _service.CreateLoyaltyTierAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpPut("loyalty-tiers/{id:guid}")]
    public async Task<IActionResult> UpdateLoyaltyTier(Guid id, UpdateLoyaltyTierRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateLoyaltyTierAsync(_currentUser.UserId, id, request, ct);
        return Ok(result);
    }

    [HttpDelete("loyalty-tiers/{id:guid}")]
    public async Task<IActionResult> DeleteLoyaltyTier(Guid id, CancellationToken ct)
    {
        await _service.DeleteLoyaltyTierAsync(_currentUser.UserId, id, ct);
        return NoContent();
    }
}
