using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Common.Interfaces.Services.Auth;
namespace Ube.Api.Controllers.Listings;
[Authorize (Roles = "Vendor")]
[ApiController]
[Route("api/listings")]
public class ListingController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ICurrentUserService _currentUser;

    public ListingController(IListingService listingService, ICurrentUserService currentUser)
    {
        _listingService = listingService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetVendorListings()
    {
        var vendorId = _currentUser.UserId;
        var listings = await _listingService.GetVendorListingsAsync(vendorId);
        return Ok(listings);
    }
}