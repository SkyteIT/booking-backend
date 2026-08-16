namespace Ube.Application.Features.Cart;

public interface ICartService
{
    Task<CartDto?> GetCartByUserIdAsync(Guid userId);
    Task<CartDto> GetOrCreateCartAsync(Guid userId);
    Task<CartDto> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(Guid userId, UpdateCartItemRequest request);
    Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId);
    Task<bool> ClearCartAsync(Guid userId);
}
