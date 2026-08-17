using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers.Reviews;
[AllowAnonymous]
[ApiController ]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly ICurrentUserService _currentUser;

    public ReviewsController(
        IReviewService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Anonymous callers can view reviews, but ICurrentUserService.UserId
    // throws for them - only read it when the request actually carries an
    // authenticated principal, so "like" state can be shown to signed-in
    // viewers without breaking anonymous browsing.
    private Guid? CurrentUserIdOrNull =>
        User.Identity?.IsAuthenticated == true ? _currentUser.UserId : null;

    // get reviews for a vendor with pagination and optional rating filter
    [HttpGet("vendors/{vendorId}/reviews")]
    public async Task<IActionResult> GetByVendor(
        Guid vendorId,
        [FromQuery] ReviewRequest options)
    {
        var result = await _service.GetReviewsByVendorAsync(vendorId, options, CurrentUserIdOrNull);
        return Ok(result);
    }
    // get average rating and total reviews for a vendor
    [HttpGet("vendors/{vendorId}/reviews/rating")]
    public async Task<IActionResult> GetRating(Guid vendorId)
    {
        var result = await _service.GetRatingAsync(vendorId);
        return Ok(result);
    }

    // get reviews for a single listing with pagination and optional rating filter
    [HttpGet("listings/{listingId}/reviews")]
    public async Task<IActionResult> GetByListing(
        Guid listingId,
        [FromQuery] ReviewRequest options)
    {
        var result = await _service.GetReviewsByListingAsync(listingId, options, CurrentUserIdOrNull);
        return Ok(result);
    }
}