using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class VendorCommissionRepository : IVendorCommissionRepository
{
    private readonly ApplicationDbContext _db;

    public VendorCommissionRepository(ApplicationDbContext db) => _db = db;

    // Category-specific override takes priority over a vendor-wide one;
    // within that, the most recently created active override wins.
    public async Task<VendorCommissionOverride?> GetActiveOverrideAsync(Guid vendorProfileId, Guid? categoryId, DateTime asOf, CancellationToken ct = default)
    {
        var candidates = await _db.VendorCommissionOverrides
            .Where(x => x.VendorProfileId == vendorProfileId
                        && x.Status == CommissionOverrideStatus.Active
                        && x.StartDate <= asOf
                        && (x.EndDate == null || x.EndDate >= asOf)
                        && (x.CategoryId == categoryId || x.CategoryId == null))
            .OrderByDescending(x => x.CategoryId != null)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return candidates.FirstOrDefault();
    }

    public async Task<IReadOnlyList<VendorCommissionOverride>> GetAllForVendorAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.VendorCommissionOverrides
            .Where(x => x.VendorProfileId == vendorProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddOverrideAsync(VendorCommissionOverride commissionOverride, CancellationToken ct = default)
    {
        await _db.VendorCommissionOverrides.AddAsync(commissionOverride, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateOverrideAsync(VendorCommissionOverride commissionOverride, CancellationToken ct = default)
    {
        _db.VendorCommissionOverrides.Update(commissionOverride);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LoyaltyDiscountTier>> GetLoyaltyTiersAsync(CancellationToken ct = default)
        => await _db.LoyaltyDiscountTiers
            .OrderByDescending(x => x.MonthsActive)
            .ToListAsync(ct);

    public async Task AddAcknowledgementAsync(VendorCommissionAcknowledgement acknowledgement, CancellationToken ct = default)
    {
        await _db.VendorCommissionAcknowledgements.AddAsync(acknowledgement, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<VendorCommissionAcknowledgement?> GetLatestAcknowledgementAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.VendorCommissionAcknowledgements
            .Where(x => x.VendorProfileId == vendorProfileId)
            .OrderByDescending(x => x.AcknowledgedAt)
            .FirstOrDefaultAsync(ct);
}
