using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ube.Application.DTOs.Cart;
using Ube.Application.Services.Cart;

namespace Ube.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
	private readonly ICartService _service;

	public CartController(ICartService service)
	{
		_service = service;
	}

	[HttpGet]
	public Task<CartDto> GetCart(CancellationToken cancellationToken) => _service.GetCartAsync(UserId(), cancellationToken);

	[HttpPost("items")]
	public Task<CartDto> AddItem(AddToCartRequest request, CancellationToken cancellationToken) => _service.AddItemAsync(UserId(), request, cancellationToken);

	[HttpPut("items")]
	public Task<CartDto> UpdateItem(UpdateCartItemRequest request, CancellationToken cancellationToken) => _service.UpdateItemAsync(UserId(), request, cancellationToken);

	[HttpDelete("items/{cartItemId:guid}")]
	public Task<CartDto> RemoveItem(Guid cartItemId, CancellationToken cancellationToken) => _service.RemoveItemAsync(UserId(), cartItemId, cancellationToken);

	[HttpDelete]
	public async Task<IActionResult> Clear(CancellationToken cancellationToken)
	{
		await _service.ClearAsync(UserId(), cancellationToken);
		return NoContent();
	}

	private Guid UserId()
	{
		var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
	}
}
