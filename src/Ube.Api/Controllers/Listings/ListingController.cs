using System.Text.Json;
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
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IListingService _listingService;
    private readonly ISeasonalPricingService _seasonalPricingService;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public ListingController(IListingService listingService, ISeasonalPricingService seasonalPricingService, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _listingService = listingService;
        _seasonalPricingService = seasonalPricingService;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
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

    // Complete, owner-checked payload for pre-filling the vendor edit form.
    // It includes base fields, existing image URLs, the selected category,
    // exactly one category-specific details object, and custom-field values.
    [HttpGet("{id:guid}/edit")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetListingForEdit(Guid id, CancellationToken ct)
    {
        var listing = await _listingService.GetListingForEditAsync(id, _currentUser.UserId, ct);
        return Ok(listing);
    }

    // `data` carries every non-file field as a JSON string (built by the same
    // frontend mapping that used to go straight in the request body) - real
    // photos ride alongside it as actual files in the same multipart request,
    // instead of vendors pasting external image URLs into a text field.
    [HttpPost]
    [Authorize(Roles = "Vendor")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateListing([FromForm] string data, [FromForm] List<IFormFile>? images, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<CreateListingRequest>(data, JsonOptions)
            ?? throw new BusinessRuleException("Invalid listing data.");

        var validation = await new CreateListingRequestValidator().ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var imageUrls = await SaveListingImagesAsync(images ?? new List<IFormFile>(), ct);
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
            await DeleteUploadedImagesAsync(imageUrls);
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

        var urls = new List<string>();

        try
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                await using var stream = file.OpenReadStream();
                var url = await _fileStorage.UploadAsync(stream, extension, file.ContentType, IFileStorageService.ImagesContainer, ct);
                urls.Add(url);
            }

            return urls;
        }
        catch
        {
            await DeleteUploadedImagesAsync(urls);
            throw;
        }
    }

    private async Task DeleteUploadedImagesAsync(IEnumerable<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
            await _fileStorage.DeleteAsync(imageUrl);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateListing(Guid id, [FromForm] string data, [FromForm] List<IFormFile>? images, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<UpdateListingRequest>(data, JsonOptions)
            ?? throw new BusinessRuleException("Invalid listing data.");

        var validation = await new UpdateListingRequestValidator().ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        // `request.Images` here is the set of existing photo URLs the vendor
        // chose to keep (already-uploaded, unchanged) - newly picked files
        // get uploaded and appended, not swapped in wholesale.
        var newImageUrls = await SaveListingImagesAsync(images ?? new List<IFormFile>(), ct);
        request.Images = request.Images.Concat(newImageUrls).ToList();

        await _listingService.UpdateListingAsync(id, _currentUser.UserId, request, ct);
        var updated = await _listingService.GetListingForEditAsync(id, _currentUser.UserId, ct);
        return Ok(updated);
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
