using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces.Repositories;
using Ube.Domain.Entities.Carts;
using Ube.Domain.Entities.Listings;
using Ube.Infrastructure.Persistence;

namespace Ube.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Listing)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetCartWithItemsAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Listing)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetByIdAsync(Guid cartId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task AddCartItemAsync(CartItem item)
{
    await _context.CartItems.AddAsync(item); // explicitly marks as Added → INSERT
}

    public Task UpdateAsync(Cart cart)
    {
        _context.Carts.Update(cart);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task<Listing?> GetListingByIdAsync(Guid listingId)
    {
        return await _context.Listings
            .FirstOrDefaultAsync(x => x.Id == listingId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}