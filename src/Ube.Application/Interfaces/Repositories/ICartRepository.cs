using Ube.Domain.Entities.Carts;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId);

    Task<Cart?> GetByIdAsync(Guid cartId);

    Task<Cart?> GetCartWithItemsAsync(Guid userId);

    Task AddAsync(Cart cart);
    Task AddCartItemAsync(CartItem item); 

    Task RemoveItemAsync(CartItem item);

    Task<Listing?> GetListingByIdAsync(Guid listingId);

    Task SaveChangesAsync();



    
}