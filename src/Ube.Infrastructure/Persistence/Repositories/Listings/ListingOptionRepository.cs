using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingOptionRepository : IListingOptionRepository
{
    private readonly ApplicationDbContext _db;

    public ListingOptionRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<ListingOptionGroup>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default)
        => await _db.ListingOptionGroups
            .Include(g => g.Values)
            .Where(g => g.ListingId == listingId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(ct);

    public async Task<ListingOptionGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken ct = default)
        => await _db.ListingOptionGroups
            .Include(g => g.Values)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public async Task AddGroupAsync(ListingOptionGroup group, CancellationToken ct = default)
    {
        await _db.ListingOptionGroups.AddAsync(group, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateGroupAsync(ListingOptionGroup group, CancellationToken ct = default)
    {
        _db.ListingOptionGroups.Update(group);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteGroupAsync(ListingOptionGroup group, CancellationToken ct = default)
    {
        _db.ListingOptionGroups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ListingOptionValue?> GetValueByIdAsync(Guid valueId, CancellationToken ct = default)
        => await _db.ListingOptionValues
            .Include(v => v.Group)
            .FirstOrDefaultAsync(v => v.Id == valueId, ct);

    public async Task AddValueAsync(ListingOptionValue value, CancellationToken ct = default)
    {
        await _db.ListingOptionValues.AddAsync(value, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateValueAsync(ListingOptionValue value, CancellationToken ct = default)
    {
        _db.ListingOptionValues.Update(value);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteValueAsync(ListingOptionValue value, CancellationToken ct = default)
    {
        _db.ListingOptionValues.Remove(value);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<ListingOptionValue>> GetValuesByIdsAsync(IEnumerable<Guid> valueIds, CancellationToken ct = default)
        => await _db.ListingOptionValues
            .Include(v => v.Group)
            .Where(v => valueIds.Contains(v.Id))
            .ToListAsync(ct);
}
