using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Cart;
using Ube.Domain.Entities.Carts;

namespace Ube.Infrastructure.Persistence.Repositories.Cart;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;

    public CartRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Ube.Domain.Entities.Carts.Cart?> GetByUserIdAsync(Guid userId)
        => await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Listing)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task AddAsync(Ube.Domain.Entities.Carts.Cart cart)
        => await _db.Carts.AddAsync(cart);

    public async Task AddCartItemAsync(CartItem item)
        => await _db.CartItems.AddAsync(item);

    public Task RemoveItemAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
