using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;
using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Users;


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
            .Include(l => l.VendorProfile)
            .FirstOrDefaultAsync(b => b.Id == listingId);
    }
    public async Task<List<Listing>> GetByVendorIdAsync(Guid vendorId)
{
    return await _db.Listings
        .Include(l => l.VendorProfile)
        .Where(l => l.VendorProfile.UserId == vendorId)
        .ToListAsync();
}
}