using Ube.Domain.Entities.Carts;

namespace Ube.Application.Features.Cart;

public interface ICartRepository
{
    Task<Ube.Domain.Entities.Carts.Cart?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Ube.Domain.Entities.Carts.Cart cart);
    Task AddCartItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task SaveChangesAsync();
}
