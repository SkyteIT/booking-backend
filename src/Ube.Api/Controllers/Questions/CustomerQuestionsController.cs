using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Questions;

namespace Ube.Api.Controllers.Questions;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/listings/{listingId}/questions")]
public class CustomerQuestionsController : ControllerBase
{
    private readonly IListingQuestionService _service;
    private readonly ICurrentUserService _currentUser;

    public CustomerQuestionsController(IListingQuestionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // Ask a question about a listing - not a review, no booking required.
    [HttpPost]
    public async Task<IActionResult> Ask(Guid listingId, [FromBody] AskQuestionDto dto)
    {
        await _service.AskQuestionAsync(listingId, dto, _currentUser.UserId);
        return Ok(new { message = "Question submitted" });
    }
}
