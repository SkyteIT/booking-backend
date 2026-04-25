using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Availability;
using Ube.Application.Common.Interfaces.Services.Auth;  
using Microsoft.AspNetCore.Authorization;
namespace Ube.Api.Controllers.Availability;
    
[Authorize]
[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _as;
    private readonly ICurrentUserService _currentUser;

    public AvailabilityController(IAvailabilityService aService, ICurrentUserService currentUser)
    {
        _as = aService;
        _currentUser = currentUser;
    }

    [HttpGet("{listingId}/calendar")]
    public async Task<IActionResult> GetCalendar(
        Guid listingId, 
        [FromQuery] int month, 
        [FromQuery] int year)
    {
        var vendorId = _currentUser.UserId;
        var result = await _as.GetCalanderAsync(listingId, vendorId, month, year);
        return Ok(result);
    }

    [HttpPost("{listingId}/block")]
    public async Task<IActionResult> BlockDates(
        Guid listingId,
        [FromBody] BlockDatesRequest request)
    {
        var vendorId = _currentUser.UserId;
        await _as.BlockdatesAsync(listingId, vendorId, request.Dates);
        return Ok("Dates blocked successfully");
        
    }

    [HttpPost("{listingId}/unblock")]
    public async Task<IActionResult> UnBlockDates(Guid listingId,
    
    [FromBody] BlockDatesRequest request)
    {
        var vendorId = _currentUser.UserId;
        await _as.UnBlockdatesAsync(listingId, vendorId, request.Dates);
        return Ok("Dates unblocked successfully");
        
    }
}