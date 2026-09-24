using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingAddonRepository : IListingAddonRepository
{
    private readonly ApplicationDbContext _context;

    public ListingAddonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ListingAddon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ListingAddons
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ListingAddon>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.ListingAddons
            .Where(x => x.ListingId == listingId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ListingAddon addon, CancellationToken cancellationToken = default)
    {
        await _context.ListingAddons.AddAsync(addon, cancellationToken);
    }

    public Task UpdateAsync(ListingAddon addon, CancellationToken cancellationToken = default)
    {
        _context.ListingAddons.Update(addon);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ListingAddon addon, CancellationToken cancellationToken = default)
    {
        _context.ListingAddons.Remove(addon);
        return Task.CompletedTask;
    }
}
