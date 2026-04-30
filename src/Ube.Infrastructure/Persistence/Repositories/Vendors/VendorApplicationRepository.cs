using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;

public class VendorApplicationRepository : IVendorApplicationRepository
{
    private readonly ApplicationDbContext _db;
    public VendorApplicationRepository(ApplicationDbContext db)    {
        _db = db;
    }

    public async Task<VendorApplication?> GetByIdAsync(Guid id)
    {
        return await _db.VendorApplications.FindAsync(id);
    }

    public async Task UpdateAsync(VendorApplication application)
    {
        _db.VendorApplications.Update(application);
        await _db.SaveChangesAsync();
    }
}