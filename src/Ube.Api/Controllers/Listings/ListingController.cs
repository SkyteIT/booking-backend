using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services;


namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings")]
public class ListingController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetVendorListings(Guid vendorId)
    {
        var listings = await _listingService.GetVendorListingsAsync(vendorId);
        return Ok(listings);
    }
}