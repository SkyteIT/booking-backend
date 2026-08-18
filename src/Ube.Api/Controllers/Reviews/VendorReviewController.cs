using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Reviews;

namespace Ube.Api.Controllers.Reviews;

[Authorize (Roles = "Vendor")]
[ApiController]
[Route("api/vendor/reviews")]
public class VendorReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly ICurrentUserService _currentUser;

    public VendorReviewsController(
        IReviewService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Reply to review
    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] VendorReplyDto dto)
    {
        await _service.AddVendorReplyAsync(id, dto, _currentUser.UserId);
        return Ok(new { message = "Reply added" });
    }
}