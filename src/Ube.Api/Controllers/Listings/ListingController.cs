using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;
namespace Ube.Api.Controllers.Listings;
[ApiController]
[Route("api/listings")]
public class ListingController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ISeasonalPricingService _seasonalPricingService;
    private readonly ICurrentUserService _currentUser;

    public ListingController(IListingService listingService, ISeasonalPricingService seasonalPricingService, ICurrentUserService currentUser)
    {
        _listingService = listingService;
        _seasonalPricingService = seasonalPricingService;
        _currentUser = currentUser;
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
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request, CancellationToken ct)
    {
        var listingId = await _listingService.CreateListingAsync(_currentUser.UserId, request, ct);
        return CreatedAtAction(nameof(GetListingById), new { id = listingId }, new { id = listingId });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request, CancellationToken ct)
    {
        await _listingService.UpdateListingAsync(id, _currentUser.UserId, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteListing(Guid id, CancellationToken ct)
    {
        await _listingService.DeleteListingAsync(id, _currentUser.UserId, ct);
        return NoContent();
    }
}