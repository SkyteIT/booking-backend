using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Common.Models;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers.Reviews;

[ApiController]
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

    // create a review for a booking - only customers can create reviews and only one review per booking
    [Authorize]
    [HttpPost("reviews")]
    // get reviews for a vendor with pagination and optional rating filter
    [HttpGet("vendors/{vendorId}/reviews")]
    public async Task<IActionResult> GetByVendor(
        Guid vendorId,
        [FromQuery] ReviewRequest options)
    {
        var result = await _service.GetReviewsByVendorAsync(vendorId, options);
        return Ok(result);
    }
    // get average rating and total reviews for a vendor
    [HttpGet("vendors/{vendorId}/reviews/rating")]
    public async Task<IActionResult> GetRating(Guid vendorId)
    {
        var result = await _service.GetRatingAsync(vendorId);
        return Ok(result);
    }

    
    
}