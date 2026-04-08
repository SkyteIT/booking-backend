using Ube.Application.Common.Interfaces.Services;
using Ube.Api.Contracts.Bookings;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Bookings;

namespace Ube.Api.Controllers.Bookings;

[ApiController]
[Route("api/vendor/bookings")]
public class VendorBookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public VendorBookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPatch("{bookingId}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        Guid bookingId,
        [FromBody] UpdateVendorBookingStatusRequest request)
    {
        var bookingDetail = await _bookingService.UpdateVendorBookingStatusAsync(
            bookingId,
            request.VendorId,
            request.NewStatus);

        if (bookingDetail == null)
            return BadRequest("Booking status update failed.");

        return Ok(bookingDetail);
    }
// get vendor bookings with status filter
    [HttpGet("vendor-booking")]
    public async Task<IActionResult> GetVendorBookings(
        [FromQuery] Guid vendorId , 
        [FromQuery] BookingsRequest request
         )
    {
        var bookings = await _bookingService.GetVendorBookingsAsync(vendorId, request);
        return Ok(bookings);
    }

    [HttpGet("booking-detail/{bookingId}")]
    public async Task<IActionResult> GetBookingDetail(
        Guid bookingId,[FromQuery] Guid vendorId)
    {
        var bookingDetail = await _bookingService.GetBookingDetailAsync(bookingId, vendorId);
        if(bookingDetail == null)
            return NotFound("Booking not found or you don't have access to it.");
        return Ok(bookingDetail);

    }
}