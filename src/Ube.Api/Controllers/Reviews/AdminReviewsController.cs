using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers.Reviews;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/reviews")]
public class AdminReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly ICurrentUserService _currentUser;

    public AdminReviewsController(
        IReviewService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // moderation queue - optionally filter by hidden status (?isHidden=true / false / omit for all)
    [HttpGet]
    public async Task<IActionResult> GetForModeration(
        [FromQuery] bool? isHidden,
        [FromQuery] ReviewRequest options)
    {
        var result = await _service.GetReviewsForModerationAsync(isHidden, options);
        return Ok(result);
    }

    [HttpPut("{reviewId}/hide")]
    public async Task<IActionResult> Hide(Guid reviewId, [FromBody] HideReviewDto dto)
    {
        await _service.HideReviewAsync(reviewId, _currentUser.UserId, dto.Reason);
        return Ok(new { message = "Review hidden" });
    }

    [HttpPut("{reviewId}/unhide")]
    public async Task<IActionResult> Unhide(Guid reviewId)
    {
        await _service.UnhideReviewAsync(reviewId, _currentUser.UserId);
        return Ok(new { message = "Review unhidden" });
    }
}
