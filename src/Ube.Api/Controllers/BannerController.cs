using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/banners")] // Base route
public class BannerController : ControllerBase
{
    private readonly IBannerService _service;
    private readonly IFileStorageService _fileStorage;

    public BannerController(IBannerService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
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
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        // Returns 201 with location header
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // POST: api/banners/upload-image
    [Authorize(Roles = "Admin")]
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            throw new BusinessRuleException("Invalid file");

        var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedTypes.Contains(extension))
            throw new BusinessRuleException("Only JPG/PNG files are allowed");

        const long maxFileSize = 2 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessRuleException("File size must not exceed 2MB");

        await using var stream = file.OpenReadStream();
        var imageUrl = await _fileStorage.UploadAsync(stream, extension, file.ContentType, IFileStorageService.ImagesContainer);

        return Ok(new { imageUrl });
    }

    // PUT: api/banners/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBannerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE: api/banners/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(); // 204 or 404
    }
}
