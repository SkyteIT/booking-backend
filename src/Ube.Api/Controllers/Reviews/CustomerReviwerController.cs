using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers;

[Authorize]
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
}