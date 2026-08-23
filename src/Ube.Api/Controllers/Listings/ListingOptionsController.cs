using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;

namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings/{listingId:guid}/options")]
public class ListingOptionsController : ControllerBase
{
    private readonly IListingOptionService _optionService;
    private readonly ICurrentUserService _currentUser;

    public ListingOptionsController(IListingOptionService optionService, ICurrentUserService currentUser)
    {
        _optionService = optionService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetGroups(Guid listingId, CancellationToken ct)
    {
        var groups = await _optionService.GetForListingAsync(listingId, ct);
        return Ok(groups);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> AddGroup(Guid listingId, AddListingOptionGroupRequest request, CancellationToken ct)
    {
        var group = await _optionService.AddGroupAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(group);
    }

    [HttpPut("{groupId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateGroup(Guid listingId, Guid groupId, UpdateListingOptionGroupRequest request, CancellationToken ct)
    {
        var group = await _optionService.UpdateGroupAsync(listingId, groupId, _currentUser.UserId, request, ct);
        return Ok(group);
    }

    [HttpDelete("{groupId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteGroup(Guid listingId, Guid groupId, CancellationToken ct)
    {
        await _optionService.DeleteGroupAsync(listingId, groupId, _currentUser.UserId, ct);
        return NoContent();
    }

    [HttpPost("{groupId:guid}/values")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> AddValue(Guid listingId, Guid groupId, AddListingOptionValueRequest request, CancellationToken ct)
    {
        var value = await _optionService.AddValueAsync(listingId, groupId, _currentUser.UserId, request, ct);
        return Ok(value);
    }

    [HttpPut("{groupId:guid}/values/{valueId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateValue(Guid listingId, Guid groupId, Guid valueId, UpdateListingOptionValueRequest request, CancellationToken ct)
    {
        var value = await _optionService.UpdateValueAsync(listingId, groupId, valueId, _currentUser.UserId, request, ct);
        return Ok(value);
    }

    [HttpDelete("{groupId:guid}/values/{valueId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteValue(Guid listingId, Guid groupId, Guid valueId, CancellationToken ct)
    {
        await _optionService.DeleteValueAsync(listingId, groupId, valueId, _currentUser.UserId, ct);
        return NoContent();
    }
}
