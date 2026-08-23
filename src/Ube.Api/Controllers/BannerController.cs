using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Vendors;
using Ube.Domain.Enums.Content;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/banners")] // Base route
public class BannerController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png"
    };
    private const long MaxFileSize = 2 * 1024 * 1024;

    private readonly IBannerService _service;
    private readonly IFileStorageService _fileStorage;

    public BannerController(IBannerService service, IFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
    }

    // GET: api/admin/banners
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("/api/admin/banners")]
    public async Task<IActionResult> GetAllAdmin(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    // GET: api/banners
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    // GET: api/admin/banners/{id}
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("/api/admin/banners/{id:guid}")]
    public async Task<IActionResult> GetByIdAdmin(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // GET: api/banners/public?placement=LandingPage&date=2026-08-22
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetActiveByPlacement(
        [FromQuery] BannerPlacement placement,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetActiveByPlacementAsync(placement, date, cancellationToken);
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
        var imageUrl = await SaveBannerImageAsync(request.File);
        return Ok(new { imageUrl });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            var data = form["data"].FirstOrDefault();
            var image = form.Files.GetFile("image") ?? form.Files.GetFile("file") ?? form.Files.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(data))
            {
                if (image is null)
                    return BadRequest("Banner data or image is required.");

                return await UpdateImageInternalAsync(id, image, cancellationToken);
            }

            var dto = JsonSerializer.Deserialize<UpdateBannerDto>(data, JsonOptions)
                ?? throw new BusinessRuleException("Invalid banner data.");

            return await UpdateBannerInternalAsync(id, dto, image, cancellationToken);
        }

        var jsonDto = await Request.ReadFromJsonAsync<UpdateBannerDto>(JsonOptions, cancellationToken);
        if (jsonDto is null)
            return BadRequest("Invalid banner data.");

        return await UpdateBannerInternalAsync(id, jsonDto, null, cancellationToken);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateImage(Guid id, [FromForm] UploadImageRequest request, CancellationToken cancellationToken)
    {
        return await UpdateImageInternalAsync(id, request.File, cancellationToken);
    }

    // DELETE: api/banners/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(); // 204 or 404
    }

    private static async Task<string> SaveBannerImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new BusinessRuleException("Invalid file");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            throw new BusinessRuleException("Only JPG/PNG files are allowed");

        if (file.Length > MaxFileSize)
            throw new BusinessRuleException("File size must not exceed 2MB");

        await using var stream = file.OpenReadStream();
        var imageUrl = await _fileStorage.UploadAsync(stream, extension, file.ContentType, IFileStorageService.ImagesContainer);

        return Ok(new { imageUrl });
    }

    private static Task DeleteBannerImageAsync(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.CompletedTask;

        if (!imageUrl.StartsWith("/images/banners/", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrWhiteSpace(fileName))
            return Task.CompletedTask;

        var filePath = Path.Combine("wwwroot", "images", "banners", fileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        return Task.CompletedTask;
    }

    private async Task<IActionResult> UpdateBannerInternalAsync(
        Guid id,
        UpdateBannerDto dto,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound();

        string? newImageUrl = null;

        try
        {
            if (image is not null && image.Length > 0)
                newImageUrl = await SaveBannerImageAsync(image);

            if (!string.IsNullOrWhiteSpace(newImageUrl))
                dto.ImageUrl = newImageUrl;

            var result = await _service.UpdateAsync(id, dto, cancellationToken);
            if (result is null)
            {
                if (!string.IsNullOrWhiteSpace(newImageUrl))
                    await DeleteBannerImageAsync(newImageUrl);
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(newImageUrl) &&
                !string.Equals(existing.ImageUrl, newImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await DeleteBannerImageAsync(existing.ImageUrl);
            }

            return Ok(result);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(newImageUrl))
                await DeleteBannerImageAsync(newImageUrl);
            throw;
        }
    }

    private async Task<IActionResult> UpdateImageInternalAsync(
        Guid id,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound();

        var newImageUrl = await SaveBannerImageAsync(image);

        try
        {
            var result = await _service.UpdateImageAsync(id, newImageUrl, cancellationToken);
            if (result is null)
            {
                await DeleteBannerImageAsync(newImageUrl);
                return NotFound();
            }

            if (!string.Equals(existing.ImageUrl, newImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                await DeleteBannerImageAsync(existing.ImageUrl);
            }

            return Ok(result);
        }
        catch
        {
            await DeleteBannerImageAsync(newImageUrl);
            throw;
        }
    }
}
