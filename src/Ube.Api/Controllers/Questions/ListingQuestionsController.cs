using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Questions;

namespace Ube.Api.Controllers.Questions;

[AllowAnonymous]
[ApiController]
[Route("api")]
public class ListingQuestionsController : ControllerBase
{
    private readonly IListingQuestionService _service;

    public ListingQuestionsController(IListingQuestionService service)
    {
        _service = service;
    }

    // get Q&A for a single listing with pagination
    [HttpGet("listings/{listingId}/questions")]
    public async Task<IActionResult> GetByListing(Guid listingId, [FromQuery] QuestionRequest options)
    {
        var result = await _service.GetQuestionsByListingAsync(listingId, options);
        return Ok(result);
    }
}
