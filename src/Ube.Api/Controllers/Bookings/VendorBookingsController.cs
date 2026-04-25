using Ube.Application.Common.Interfaces.Services;
using Ube.Api.Contracts.Bookings;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Bookings;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Common.Interfaces.Services.Auth;

namespace Ube.Api.Controllers.Bookings;

[Authorize]
[ApiController]
[Route("api/vendor/bookings")]
public class VendorBookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUser;

    public VendorBookingsController(IBookingService bookingService, ICurrentUserService currentUser)
    {
        _bookingService = bookingService;
        _currentUser = currentUser;
    }

    [HttpPatch("{bookingId}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        Guid bookingId,
        [FromBody] UpdateVendorBookingStatusRequest request)
    {
        var vendorId = _currentUser.UserId;
        var bookingDetail = await _bookingService.UpdateVendorBookingStatusAsync(
            bookingId,
            vendorId,
            request.NewStatus);

        if (bookingDetail == null)
            return BadRequest("Booking status update failed.");

        return Ok(bookingDetail);
    }
// get vendor bookings with status filter
    [HttpGet]
    public async Task<IActionResult> GetVendorBookings( 
        [FromQuery] BookingsRequest request
         )
    {
        var vendorId = _currentUser.UserId;
        var bookings = await _bookingService.GetVendorBookingsAsync(vendorId, request);
        return Ok(bookings);
    }

    [HttpGet("{bookingId}")]
    public async Task<IActionResult> GetBookingDetail(
        Guid bookingId)
    {
        var vendorId = _currentUser.UserId;
        var bookingDetail = await _bookingService.GetBookingDetailAsync(bookingId, vendorId);
        if(bookingDetail == null)
            return NotFound("Booking not found or you don't have access to it.");
        return Ok(bookingDetail);

    }
}