using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _db;

    public ListingRepository(ApplicationDbContext db) => _db = db;

    public async Task<Listing?> GetByIdAsync(Guid listingId)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .FirstOrDefaultAsync(l => l.Id == listingId);

    public async Task<List<Listing>> GetByVendorIdAsync(Guid vendorId)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .Where(l => l.VendorProfile.UserId == vendorId)
            .ToListAsync();

    public async Task UpdateAsync(Listing listing)
    {
        _db.Listings.Update(listing);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Listings
            .Where(l => l.CategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<List<Listing>> GetOrphanedByCategoryNameAsync(Guid uncategorizedId, string originalName, CancellationToken ct = default)
        => await _db.Listings
            .Where(l => l.CategoryId == uncategorizedId
                     && l.OriginalCategoryName != null
                     && l.OriginalCategoryName.ToLower() == originalName.ToLower())
            .ToListAsync(ct);
}
