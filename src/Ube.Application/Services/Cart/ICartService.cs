using Ube.Application.DTOs.Cart;

namespace Ube.Application.Services.Cart;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(Guid userId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateItemAsync(Guid userId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task ClearAsync(Guid userId, CancellationToken cancellationToken = default);
}