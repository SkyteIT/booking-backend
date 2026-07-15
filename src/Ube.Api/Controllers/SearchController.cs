using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Search;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/search")] // Base route
public class SearchController : ControllerBase
{
    private readonly ISearchService _service;

    public SearchController(ISearchService service)
    {
        _service = service;
    }

    // GET: api/search/listings?query=...&filters=...
    [HttpGet("listings")]
    public async Task<IActionResult> Search(
        [FromQuery] SearchListingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.SearchAsync(request, cancellationToken);
        return Ok(result);
    }
}