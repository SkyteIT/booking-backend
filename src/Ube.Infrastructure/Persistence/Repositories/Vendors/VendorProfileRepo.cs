using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Vendors;
using Ube.Application.Features.Vendors;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;
public class VendorProfileRepository : IVendorProfileRepository
{
    private readonly ApplicationDbContext _db;

    public VendorProfileRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    // Method to get vendor profile by user ID
    public async Task<VendorProfile?> GetVendorIdAsync(Guid userId)
    {
        return await _db.VendorProfiles
        .AsNoTracking()
        .FirstOrDefaultAsync(v => v.UserId == userId);
    }
    // Method to update vendor profile
    public async Task UpdateAsync(VendorProfile profile)
    {
        _db.VendorProfiles.Update(profile);
        await _db.SaveChangesAsync();
    }
    // Method to add new vendor profile
    public async Task AddAsync(VendorProfile profile)
    {
        await _db.VendorProfiles.AddAsync(profile);
        await _db.SaveChangesAsync();
    }

}