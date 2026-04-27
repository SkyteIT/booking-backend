using Microsoft.AspNetCore.Mvc;
using Ube.Application.DTOs.Cart;
using Ube.Application.Services.Cart;

namespace Ube.Api.Controllers;

/// <summary>
/// Cart Controller - Handles all cart-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// GET api/cart/{userId}
    /// Retrieves the cart for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The user's cart with all items</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<CartDto>> GetCart(Guid userId)
    {
        try
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return NotFound(new { message = $"Cart not found for user {userId}" });
            }
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// GET api/cart/{userId}/or-create
    /// Gets the user's cart or creates a new one if it doesn't exist
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The user's cart (existing or newly created)</returns>
    [HttpGet("{userId}/or-create")]
    public async Task<ActionResult<CartDto>> GetOrCreateCart(Guid userId)
    {
        try
        {
            var cart = await _cartService.GetOrCreateCartAsync(userId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST api/cart/{userId}/add-item
    /// Adds an item to the user's cart
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The add to cart request containing listing ID and quantity</param>
    /// <returns>The updated cart</returns>
  
 /*
    [HttpPost("{userId}/add-item")]
    public async Task<ActionResult<CartDto>> AddToCart(Guid userId, [FromBody] AddToCartRequest request)
    {
        try
        {
            if (request == null || request.ListingId == Guid.Empty || request.Quantity <= 0)
            {
                return BadRequest(new { message = "Invalid request. ListingId must not be empty and Quantity must be greater than 0." });
            }

            var cart = await _cartService.AddToCartAsync(userId, request);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
*/

[HttpPost("{userId}/add-item")]
public async Task<ActionResult<CartDto>> AddToCart(Guid userId, [FromBody] AddToCartRequest request)
{
    try
    {
        if (request == null || request.ListingId == Guid.Empty || request.Quantity <= 0)
            return BadRequest(new { message = "ListingId must not be empty and Quantity must be greater than 0." });

        var cart = await _cartService.AddToCartAsync(userId, request);
        return Ok(cart);
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        // TEMPORARY - remove after debugging
        return BadRequest(new { 
            message    = ex.Message,

        });
    }
}


    /// <summary>
    /// PUT api/cart/{userId}/update-item
    /// Updates the quantity of an item in the cart
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The update cart item request with new quantity</param>
    /// <returns>The updated cart</returns>
    [HttpPut("{userId}/update-item")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid userId, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            if (request == null || request.CartItemId == Guid.Empty || request.Quantity <= 0)
            {
                return BadRequest(new { message = "Invalid request. CartItemId must not be empty and Quantity must be greater than 0." });
            }

            var cart = await _cartService.UpdateCartItemAsync(userId, request);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE api/cart/{userId}/remove-item/{cartItemId}
    /// Removes a specific item from the user's cart
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cartItemId">The ID of the cart item to remove</param>
    /// <returns>Success message</returns>
    [HttpDelete("{userId}/remove-item/{cartItemId}")]
    public async Task<ActionResult> RemoveFromCart(Guid userId, Guid cartItemId)
    {
        try
        {
            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            if (!result)
            {
                return NotFound(new { message = $"Cart item {cartItemId} not found for user {userId}" });
            }
            return Ok(new { message = "Item removed from cart successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE api/cart/{userId}/clear
    /// Clears all items from the user's cart
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>Success message</returns>
    [HttpDelete("{userId}/clear")]
    public async Task<ActionResult> ClearCart(Guid userId)
    {
        try
        {
            var result = await _cartService.ClearCartAsync(userId);
            if (!result)
            {
                return NotFound(new { message = $"Cart not found for user {userId}" });
            }
            return Ok(new { message = "Cart cleared successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
