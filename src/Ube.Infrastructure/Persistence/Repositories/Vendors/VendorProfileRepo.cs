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
    // Method to get vendor profile by its own ID
    public async Task<VendorProfile?> GetByIdAsync(Guid vendorProfileId)
    {
        return await _db.VendorProfiles
        .AsNoTracking()
        .FirstOrDefaultAsync(v => v.Id == vendorProfileId);
    }
    // Method to list every vendor profile (admin-facing pickers/listings)
    public async Task<IReadOnlyList<VendorProfile>> GetAllAsync()
    {
        return await _db.VendorProfiles
        .AsNoTracking()
        .Include(v => v.User)
        .OrderBy(v => v.BusinessName)
        .ToListAsync();
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