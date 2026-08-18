using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Listings;

namespace Ube.Api.Controllers.Listings;

[ApiController]
[Route("api/listings/{listingId:guid}/offers")]
public class ListingOffersController : ControllerBase
{
    private readonly IListingOfferService _offerService;
    private readonly ICurrentUserService _currentUser;

    public ListingOffersController(IListingOfferService offerService, ICurrentUserService currentUser)
    {
        _offerService = offerService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetOffers(Guid listingId, CancellationToken ct)
    {
        var offers = await _offerService.GetForListingAsync(listingId, ct);
        return Ok(offers);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> CreateOffer(Guid listingId, CreateListingOfferRequest request, CancellationToken ct)
    {
        var offer = await _offerService.CreateAsync(listingId, _currentUser.UserId, request, ct);
        return Ok(offer);
    }

    [HttpPut("{offerId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateOffer(Guid listingId, Guid offerId, UpdateListingOfferRequest request, CancellationToken ct)
    {
        var offer = await _offerService.UpdateAsync(listingId, offerId, _currentUser.UserId, request, ct);
        return Ok(offer);
    }

    [HttpDelete("{offerId:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> DeleteOffer(Guid listingId, Guid offerId, CancellationToken ct)
    {
        await _offerService.DeleteAsync(listingId, offerId, _currentUser.UserId, ct);
        return NoContent();
    }
}
