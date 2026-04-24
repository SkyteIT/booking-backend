using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Category;
using Ube.Application.Interfaces;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    /// <summary>Get all non-deleted categories (no filter).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get categories with optional search and status filter.
    /// Matches the frontend search/filter bar on the redesigned Content Management page.
    /// Query params: ?status=Active|Inactive &amp; search=hotels
    /// </summary>
    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetFilteredAsync(status, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single category by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Create a new category.
    /// Body maps directly to the AddCategory form fields.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing category.
    /// All fields are optional — only provided fields are changed (PATCH semantics over PUT route).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Soft-delete a category (sets Status = Deleted, data is preserved).
    /// Triggered by the delete confirmation dialog on the frontend.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Toggle a category Active/Inactive.
    /// Triggered by the Switch toggle on each CategoryCard.
    /// Body: { "isActive": true|false }
    /// </summary>
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

/// <summary>Request body for the PATCH /status endpoint.</summary>
public record ToggleCategoryStatusDto(bool IsActive);
