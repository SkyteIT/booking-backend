using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Listings;
using Ube.Application.Services.Listings;
using System.Security.Claims;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingService _service;

    public ListingsController(IListingService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListingResponseDto>>> GetListings(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetActiveListingsAsync(cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingResponseDto>> GetListing(Guid id, CancellationToken cancellationToken)
    {
        var listing = await _service.GetActiveListingByIdAsync(id, cancellationToken);
        return listing is null ? NotFound() : Ok(listing);
    }

    [Authorize(Roles = "Vendor")]
    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<ListingResponseDto>>> GetMyListings(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetVendorListingsAsync(UserId(), cancellationToken));
    }

    [Authorize(Roles = "Vendor")]
    [HttpPost]
    public async Task<ActionResult<ListingResponseDto>> Create(CreateListingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _service.CreateAsync(UserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Vendor")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingResponseDto>> Update(Guid id, CreateListingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _service.UpdateAsync(UserId(), id, request, cancellationToken);
            return listing is null ? NotFound() : Ok(listing);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [Authorize(Roles = "Vendor")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await _service.DeleteAsync(UserId(), id, cancellationToken) ? NoContent() : NotFound();
    }

    private Guid UserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}