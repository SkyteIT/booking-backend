using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;
using Ube.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _db;
    public ListingRepository(ApplicationDbContext db)
    {
        _db = db;
    }

   public async Task<Listing?> GetByIdAsync(Guid listingId)
    {
        return await _db.Listings
            .FirstOrDefaultAsync(b => b.Id == listingId);
    }
    public async Task<List<Listing>> GetByVendorIdAsync(Guid vendorId)
{
    return await _db.Listings
        .Where(l => l.VendorProfileId == vendorId)
        .ToListAsync();
}
}