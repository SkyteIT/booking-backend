using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Questions;

namespace Ube.Api.Controllers.Questions;

[Authorize(Roles = "Vendor")]
[ApiController]
[Route("api/vendor/questions")]
public class VendorQuestionsController : ControllerBase
{
    private readonly IListingQuestionService _service;
    private readonly ICurrentUserService _currentUser;

    public VendorQuestionsController(IListingQuestionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // All questions across the vendor's own listings, newest first.
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] QuestionRequest options)
    {
        var result = await _service.GetQuestionsByVendorAsync(_currentUser.UserId, options);
        return Ok(result);
    }

    // Answer a question left on one of the vendor's own listings.
    [HttpPost("{id}/answer")]
    public async Task<IActionResult> Answer(Guid id, [FromBody] AnswerQuestionDto dto)
    {
        await _service.AnswerQuestionAsync(id, dto, _currentUser.UserId);
        return Ok(new { message = "Answer submitted" });
    }
}
