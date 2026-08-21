using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;
using Ube.Application.Features.Listings.Validators;
using Ube.Application.Common.Exceptions;
namespace Ube.Api.Controllers.Listings;
[ApiController]
[Route("api/listings")]
public class ListingController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ISeasonalPricingService _seasonalPricingService;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _environment;

    public ListingController(IListingService listingService, ISeasonalPricingService seasonalPricingService, ICurrentUserService currentUser, IWebHostEnvironment environment)
    {
        _listingService = listingService;
        _seasonalPricingService = seasonalPricingService;
        _currentUser = currentUser;
        _environment = environment;
    }

    // Public price preview - reuses the exact same seasonal-aware
    // calculation CheckoutService uses, so this always matches the real
    // charge instead of drifting from a separate client-side estimate.
    [HttpGet("{id:guid}/price-quote")]
    public async Task<IActionResult> GetPriceQuote(
        Guid id,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? unitId,
        [FromQuery] int quantity = 1,
        CancellationToken ct = default)
    {
        var quote = await _seasonalPricingService.GetPriceQuoteAsync(id, unitId, startDate, endDate, quantity, ct);
        return Ok(quote);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetMyListings(CancellationToken ct)
    {
        var listings = await _listingService.GetMyListingsAsync(_currentUser.UserId, ct);
        return Ok(listings);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllListings(CancellationToken ct)
    {
        var listings = await _listingService.GetAllListingsAsync(ct);
        return Ok(listings);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetListingById(Guid id, CancellationToken ct)
    {
        var listing = await _listingService.GetListingByIdAsync(id, ct);
        return listing == null ? NotFound() : Ok(listing);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateListing([FromForm] CreateListingFormRequest form, CancellationToken ct)
    {
        var request = form.ToApplicationRequest(new List<string>());
        var validation = await new CreateListingRequestValidator().ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var imageUrls = await SaveListingImagesAsync(form.Images, ct);
        request.Images = imageUrls;

        try
        {
            var listingId = await _listingService.CreateListingAsync(
                _currentUser.UserId,
                request,
                ct);

            return CreatedAtAction(nameof(GetListingById), new { id = listingId }, new { id = listingId });
        }
        catch
        {
            DeleteUploadedImages(imageUrls);
            throw;
        }
    }

    private async Task<List<string>> SaveListingImagesAsync(IEnumerable<IFormFile> images, CancellationToken ct)
    {
        const long maxFileSize = 5 * 1024 * 1024;
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "image/jpeg", "image/png", "image/webp" };
        var files = images.Where(file => file.Length > 0).ToList();

        if (files.Count > 10)
            throw new BusinessRuleException("A listing can have at most 10 images.");

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(file.ContentType))
                throw new BusinessRuleException("Only JPG, PNG, and WebP images are allowed.");
            if (file.Length > maxFileSize)
                throw new BusinessRuleException("Each image must not exceed 5MB.");
        }

        var folderPath = Path.Combine(_environment.WebRootPath, "images", "listings");
        Directory.CreateDirectory(folderPath);
        var urls = new List<string>();

        try
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                await using var stream = new FileStream(
                    Path.Combine(folderPath, fileName), FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream, ct);
                urls.Add($"/images/listings/{fileName}");
            }

            return urls;
        }
        catch
        {
            DeleteUploadedImages(urls);
            throw;
        }
    }

    private void DeleteUploadedImages(IEnumerable<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
        {
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "images", "listings", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request, CancellationToken ct)
    {
        await _listingService.UpdateListingAsync(id, _currentUser.UserId, request, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/publish")]
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> PublishListing(Guid id, CancellationToken ct)
    {
        await _listingService.SetPublishedAsync(id, _currentUser.UserId, true, ct);
        return Ok(new { id, isActive = true });
    }

    [HttpPatch("{id:guid}/unpublish")]
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UnpublishListing(Guid id, CancellationToken ct)
    {
        await _listingService.SetPublishedAsync(id, _currentUser.UserId, false, ct);
        return Ok(new { id, isActive = false });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteListing(Guid id, CancellationToken ct)
    {
        await _listingService.DeleteListingAsync(id, _currentUser.UserId, ct);
        return NoContent();
    }
}
