using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Availability;

namespace Ube.Api.Controllers.Availability;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _as;

    public AvailabilityController(IAvailabilityService aService)
    {
        _as = aService;
    }

    [HttpGet("{listingId}/calendar")]
    public async Task<IActionResult> GetCalendar(
        Guid listingId,
        [FromQuery] Guid vendorId, 
        [FromQuery] int month, 
        [FromQuery] int year)
    {
        var result = await _as.GetCalanderAsync(listingId, vendorId, month, year);
        return Ok(result);
    }

    [HttpPost("{listingId}/block")]
    public async Task<IActionResult> BlockDates(
        Guid listingId, 
        [FromQuery] Guid vendorId,
        [FromBody] BlockDatesRequest request)
    {
        await _as.BlockdatesAsync(listingId, vendorId, request.Dates);
        return Ok("Dates blocked successfully");
        
    }

    [HttpPost("{listingId}/unblock")]
    public async Task<IActionResult> UnBlockDates(Guid listingId,
    [FromQuery] Guid vendorId,
    [FromBody] BlockDatesRequest request)
    {
       
        await _as.UnBlockdatesAsync(listingId, vendorId, request.Dates);
        return Ok("Dates unblocked successfully");
        
    }
}