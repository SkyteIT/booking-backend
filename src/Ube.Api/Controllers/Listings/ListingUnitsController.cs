using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;

namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings/{listingId:guid}/units")]
public class ListingUnitsController : ControllerBase
{
    private readonly IListingUnitService _unitService;
    private readonly ICurrentUserService _currentUser;

    public ListingUnitsController(IListingUnitService unitService, ICurrentUserService currentUser)
    {
        _unitService = unitService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnits(Guid listingId, CancellationToken ct)
    {
        var units = await _unitService.GetForListingAsync(listingId, ct);
        return Ok(units);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> AddUnit(Guid listingId, AddListingUnitRequest request, CancellationToken ct)
    {
        var unit = await _unitService.AddAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(unit);
    }

    [HttpPost("bulk-grid")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> AddGrid(Guid listingId, AddListingUnitsGridRequest request, CancellationToken ct)
    {
        var units = await _unitService.AddGridAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(units);
    }

    [HttpPost("bulk-timeslots")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> AddTimeSlots(Guid listingId, AddListingUnitsTimeSlotsRequest request, CancellationToken ct)
    {
        var units = await _unitService.AddTimeSlotsAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(units);
    }

    [HttpPut("{unitId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateUnit(Guid listingId, Guid unitId, UpdateListingUnitRequest request, CancellationToken ct)
    {
        var unit = await _unitService.UpdateAsync(listingId, unitId, _currentUser.UserId, request, ct);
        return Ok(unit);
    }

    [HttpDelete("{unitId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteUnit(Guid listingId, Guid unitId, CancellationToken ct)
    {
        await _unitService.DeleteAsync(listingId, unitId, _currentUser.UserId, ct);
        return NoContent();
    }
}
