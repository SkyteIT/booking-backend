using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Search;
using Ube.Application.Interfaces;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _service;

    public SearchController(ISearchService service)
    {
        _service = service;
    }

    [HttpGet("listings")]
    public async Task<IActionResult> Search([FromQuery] SearchListingsRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.SearchAsync(request, cancellationToken);
        return Ok(result);
    }
}
