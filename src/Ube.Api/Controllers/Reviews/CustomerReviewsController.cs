using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers;

[Authorize (Roles = "User")]
[ApiController]
[Route("api/reviews")]
public class CustomerReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly ICurrentUserService _currentUser;

    public CustomerReviewsController(
        IReviewService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Get the current customer's own reviews - "My Reviews" page
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] ReviewRequest options)
    {
        var result = await _service.GetMyReviewsAsync(_currentUser.UserId, options);
        return Ok(result);
    }

    //Create review
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        await _service.CreateReviewAsync(dto, _currentUser.UserId);
        return Ok(new { message = "Review created successfully" });
    }

    // Update review
    [HttpPut("{reviewId}")]
    public async Task<IActionResult> Update(Guid reviewId, [FromBody] CreateReviewDto dto)
    {
        await _service.UpdateReviewAsync(dto, _currentUser.UserId, reviewId);
        return Ok(new { message = "Review updated successfully" });
    }

    // Delete review
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteReviewAsync(id, _currentUser.UserId);
        return Ok(new { message = "Review deleted" });
    }

    // Toggle a like on a review. Lives here (not on the AllowAnonymous
    // ReviewsController) because a controller-level [AllowAnonymous]
    // always wins over an action-level [Authorize] in ASP.NET Core.
    [HttpPost("{reviewId}/like")]
    public async Task<IActionResult> ToggleLike(Guid reviewId)
    {
        var (likeCount, isLiked) = await _service.ToggleLikeAsync(reviewId, _currentUser.UserId);
        return Ok(new { likeCount, isLiked });
    }
}