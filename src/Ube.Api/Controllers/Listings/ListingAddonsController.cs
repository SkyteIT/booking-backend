using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;

namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings/{listingId:guid}/addons")]
public class ListingAddonsController : ControllerBase
{
    private readonly IListingAddonService _addonService;
    private readonly ICurrentUserService _currentUser;

    public ListingAddonsController(IListingAddonService addonService, ICurrentUserService currentUser)
    {
        _addonService = addonService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddons(Guid listingId, CancellationToken ct)
    {
        var addons = await _addonService.GetForListingAsync(listingId, ct);
        return Ok(addons);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> CreateAddon(Guid listingId, CreateListingAddonRequest request, CancellationToken ct)
    {
        var addon = await _addonService.CreateAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(addon);
    }

    [HttpPut("{addonId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateAddon(Guid listingId, Guid addonId, UpdateListingAddonRequest request, CancellationToken ct)
    {
        var addon = await _addonService.UpdateAsync(listingId, addonId, _currentUser.UserId, request, ct);
        return Ok(addon);
    }

    [HttpDelete("{addonId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteAddon(Guid listingId, Guid addonId, CancellationToken ct)
    {
        await _addonService.DeleteAsync(listingId, addonId, _currentUser.UserId, ct);
        return NoContent();
    }
}
