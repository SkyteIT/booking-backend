using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;

namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings/{listingId:guid}/seasonal-rates")]
public class SeasonalPricingController : ControllerBase
{
    private readonly ISeasonalPricingService _seasonalPricingService;
    private readonly ICurrentUserService _currentUser;

    public SeasonalPricingController(ISeasonalPricingService seasonalPricingService, ICurrentUserService currentUser)
    {
        _seasonalPricingService = seasonalPricingService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules(Guid listingId, CancellationToken ct)
    {
        var rules = await _seasonalPricingService.GetForListingAsync(listingId, ct);
        return Ok(rules);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> CreateRule(Guid listingId, CreateSeasonalPricingRuleRequest request, CancellationToken ct)
    {
        var rule = await _seasonalPricingService.CreateAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(rule);
    }

    [HttpPut("{ruleId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateRule(Guid listingId, Guid ruleId, UpdateSeasonalPricingRuleRequest request, CancellationToken ct)
    {
        var rule = await _seasonalPricingService.UpdateAsync(listingId, ruleId, _currentUser.UserId, request, ct);
        return Ok(rule);
    }

    [HttpDelete("{ruleId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteRule(Guid listingId, Guid ruleId, CancellationToken ct)
    {
        await _seasonalPricingService.DeleteAsync(listingId, ruleId, _currentUser.UserId, ct);
        return NoContent();
    }
}
