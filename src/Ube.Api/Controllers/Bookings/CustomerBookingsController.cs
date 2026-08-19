using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Bookings;

namespace Ube.Api.Controllers.Bookings;

// Any authenticated account can book/browse as a customer - a vendor
// shopping for themselves is a real, expected scenario, not just plain
// "User" accounts. Every action below is self-scoped by the caller's own
// id, so widening this carries no cross-account data risk.
[Authorize(Roles = "User,Vendor,Admin,Finance,SuperAdmin")]
[ApiController]
[Route("api/bookings")]
public class CustomerBookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICheckoutService _checkoutService;
    private readonly ICurrentUserService _currentUser;

    public CustomerBookingsController(
        IBookingService bookingService,
        ICheckoutService checkoutService,
        ICurrentUserService currentUser)
    {
        _bookingService = bookingService;
        _checkoutService = checkoutService;
        _currentUser = currentUser;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var result = await _checkoutService.CheckoutAsync(_currentUser.UserId, request, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingsRequest request)
    {
        var bookings = await _bookingService.GetCustomerBookingsAsync(_currentUser.UserId, request);
        return Ok(bookings);
    }

    [HttpGet("{bookingId}")]
    public async Task<IActionResult> GetMyBookingDetail(Guid bookingId)
    {
        var result = await _bookingService.GetCustomerBookingDetailAsync(bookingId, _currentUser.UserId);
        return Ok(result);
    }

    [HttpPatch("{bookingId}/cancel")]
    public async Task<IActionResult> CancelMyBooking(Guid bookingId)
    {
        var result = await _bookingService.CancelBookingAsync(bookingId, _currentUser.UserId);
        return Ok(result);
    }
}
