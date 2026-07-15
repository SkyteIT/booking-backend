using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Cart;

namespace Ube.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ICurrentUserService _currentUser;

    public CartController(ICartService cartService, ICurrentUserService currentUser)
    {
        _cartService = cartService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cart = await _cartService.GetOrCreateCartAsync(_currentUser.UserId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        var cart = await _cartService.AddToCartAsync(_currentUser.UserId, request);
        return Ok(cart);
    }

    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        var cart = await _cartService.UpdateCartItemAsync(_currentUser.UserId, request);
        return Ok(cart);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
    {
        var result = await _cartService.RemoveFromCartAsync(_currentUser.UserId, cartItemId);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCartAsync(_currentUser.UserId);
        return result ? NoContent() : NotFound();
    }
}
