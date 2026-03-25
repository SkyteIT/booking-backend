using Ube.Application.common.Interfaces.Services;
using Ube.Api.Contracts.Bookings;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPut("{bookingId}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        Guid bookingId,
        UpdateVendorBookingStatusRequest request)
    {
        var success = await _bookingService.UpdateVendorBookingStatusAsync(
            bookingId,
            request.VendorId,
            request.NewStatus);

        if (!success)
            return BadRequest("Booking status update failed.");

        return Ok("Booking status updated successfully.");
    }

    [HttpGet("vendor-booking")]
    public async Task<IActionResult> GetVendorBookings([FromQuery] Guid vendorId)
    {
        var bookings = await _bookingService.GetVendorBookingsAsync(vendorId);
        return Ok(bookings);
    }
}