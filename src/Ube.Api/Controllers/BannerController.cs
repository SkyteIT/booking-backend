using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Content.Banner;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/banners")] // Base route
public class BannerController : ControllerBase
{
    private readonly IBannerService _service;

    public BannerController(IBannerService service)
    {
        _service = service;
    }

    // GET: api/banners
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    // GET: api/banners/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result); // 404 if not found
    }

    // POST: api/banners
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        // Returns 201 with location header
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT: api/banners/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE: api/banners/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(); // 204 or 404
    }
}