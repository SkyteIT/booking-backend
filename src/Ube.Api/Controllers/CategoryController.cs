using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Content.Category;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/categories")] // Base route
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    // GET: api/categories/filter?status=Active&search=hotels
    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetFilteredAsync(status, search, cancellationToken);
        return Ok(result);
    }

    // GET: api/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // POST: api/categories
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        // 201 Created with location
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT: api/categories/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE: api/categories/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(); // 204 or 404
    }

    // PATCH: api/categories/{id}/status
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(
        Guid id,
        [FromBody] ToggleCategoryStatusDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.ToggleStatusAsync(id, dto.IsActive, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}

// Request body for status toggle
public record ToggleCategoryStatusDto(bool IsActive);