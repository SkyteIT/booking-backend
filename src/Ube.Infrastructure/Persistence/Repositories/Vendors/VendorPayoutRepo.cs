using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Vendors.Payout;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;

public class VendorPayoutRepository : IVendorPayoutRepository
{
    private readonly ApplicationDbContext _db;

    public VendorPayoutRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<VendorPayout?> GetByVendorIdAsync(Guid vendorId)
    {
        return await _db.VendorPayouts
            .FirstOrDefaultAsync(p => p.VendorProfileId == vendorId);
    }

    public async Task AddAsync(VendorPayout payout)
    {
        _db.VendorPayouts.Add(payout);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(VendorPayout payout)
    {
        _db.VendorPayouts.Update(payout);
        await _db.SaveChangesAsync();
    }
}