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

    public async Task<VendorProfile?> GetVendorIdAsync(Guid userId)
    {
        return await _db.VendorProfiles
            .FirstOrDefaultAsync(v => v.UserId == userId);
    }

    public async Task UpdateAsync(VendorProfile profile)
    {
        _db.VendorProfiles.Update(profile);
        await _db.SaveChangesAsync();
    }
}