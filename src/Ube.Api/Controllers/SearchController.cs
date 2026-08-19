using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Listings;
using Ube.Application.Services.Listings;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly IListingService _service;

    public SearchController(IListingService service) => _service = service;

    [AllowAnonymous]
    [HttpGet("listings")]
    public async Task<ActionResult<IReadOnlyList<ListingResponseDto>>> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] string? location,
        [FromQuery] Guid[]? categoryIds,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] double? minRating,
        [FromQuery] bool? isAvailable,
        CancellationToken cancellationToken)
    {
        var listings = await _service.GetActiveListingsAsync(cancellationToken);
        var results = listings.Where(listing =>
                (string.IsNullOrWhiteSpace(searchTerm) || listing.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || listing.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(location) || (listing.Location?.Contains(location, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (categoryIds is null || categoryIds.Length == 0 || categoryIds.Contains(listing.CategoryId)) &&
                (!minPrice.HasValue || listing.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || listing.Price <= maxPrice.Value) &&
                (!minRating.HasValue || listing.AverageRating >= minRating.Value) &&
                (!isAvailable.HasValue || listing.IsActive == isAvailable.Value))
            .ToList();
        return Ok(results);
    }
}