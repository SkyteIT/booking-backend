//using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Cart;
using Ube.Domain.Entities.Carts;
//using Ube.Infrastructure.Persistence;
using Ube.Application.Interfaces.Repositories;



namespace Ube.Application.Services.Cart;

public class CartService : ICartService
{
    /*
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    */

    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }


    /// <summary>
    /// Gets the cart for a specific user.
    /// </summary>
    public async Task<CartDto?> GetCartByUserIdAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        return cart == null ? null : MapToDto(cart);
    }

    /// <summary>
    /// Gets an existing cart or creates a new one if it doesn't exist.
    /// </summary>
    public async Task<CartDto> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if (cart != null)
            return MapToDto(cart);

        cart = new Domain.Entities.Carts.Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalPrice = 0,
            ItemCount = 0,
            CreatedAt = DateTime.UtcNow,
            Items = new List<CartItem>()
        };

        await _cartRepository.AddAsync(cart);
        await _cartRepository.SaveChangesAsync();

        return MapToDto(cart);
    }





    /// <summary>
    /// Adds an item to the user's cart.
    /// </summary>
public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartRequest request)
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);

    if (cart == null)
    {
        cart = new Domain.Entities.Carts.Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalPrice = 0,
            ItemCount = 0,
            CreatedAt = DateTime.UtcNow,
            Items = new List<CartItem>()
        };

        await _cartRepository.AddAsync(cart);
        await _cartRepository.SaveChangesAsync();
    }



    var listing = await _cartRepository.GetListingByIdAsync(request.ListingId);
    if (listing == null)
        throw new InvalidOperationException($"Listing with ID {request.ListingId} not found.");

    var existingItem = cart.Items.FirstOrDefault(i => i.ListingId == request.ListingId);

    if (existingItem != null)
    {
        existingItem.Quantity += request.Quantity;
        existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
    }
    else
    {
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ListingId = request.ListingId,
            Quantity = request.Quantity,
            UnitPrice = listing.Price,
            TotalPrice = request.Quantity * listing.Price,
            AddedAt = DateTime.UtcNow
        };

        await _cartRepository.AddCartItemAsync(cartItem);
        cart.Items.Add(cartItem);
        
    }

    UpdateCartTotals(cart);

    await _cartRepository.SaveChangesAsync();

    return MapToDto(cart);
}

    /// <summary>
    /// Updates the quantity of an item in the cart.
    /// </summary>
public async Task<CartDto> UpdateCartItemAsync(Guid userId, UpdateCartItemRequest request)
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);

    if (cart == null)
        throw new InvalidOperationException($"Cart not found for user {userId}.");

    var cartItem = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);

    if (cartItem == null)
        throw new InvalidOperationException($"Cart item with ID {request.CartItemId} not found.");


    if (request.Quantity <= 0)
        throw new InvalidOperationException("Quantity must be greater than 0.");

    cartItem.Quantity = request.Quantity;
    cartItem.TotalPrice = request.Quantity * cartItem.UnitPrice;

    UpdateCartTotals(cart);

    await _cartRepository.SaveChangesAsync();

    return MapToDto(cart);
}



    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);

    if (cart == null)
        return false;

    var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

    if (cartItem == null)
        return false;

    cart.Items.Remove(cartItem);

    await _cartRepository.RemoveItemAsync(cartItem);

    UpdateCartTotals(cart);

    await _cartRepository.SaveChangesAsync();

    return true;
}



    /// <summary>
    /// Clears all items from the user's cart.
    /// </summary>
    public async Task<bool> ClearCartAsync(Guid userId)
{
    var cart = await _cartRepository.GetByUserIdAsync(userId);

    if (cart == null)
        return false;

    foreach (var item in cart.Items.ToList())
    {
        await _cartRepository.RemoveItemAsync(item);
    }

    //cart.Items.Clear();

    cart.TotalPrice = 0;
    cart.ItemCount = 0;
    cart.UpdatedAt = DateTime.UtcNow;

    await _cartRepository.SaveChangesAsync();

    return true;
}




    // Helper methods
    private void UpdateCartTotals(Domain.Entities.Carts.Cart cart)
    {
        cart.TotalPrice = cart.Items.Sum(i => i.TotalPrice);
        cart.ItemCount = cart.Items.Sum(i => i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;
    }

    private CartDto MapToDto(Domain.Entities.Carts.Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            TotalPrice = cart.TotalPrice,
            Currency = cart.Currency,
            ItemCount = cart.ItemCount,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ListingId = i.ListingId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                AddedAt = i.AddedAt
            }).ToList()
        };
    }
}
