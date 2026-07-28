using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

// Insert-only by construction - mirrors ILedgerRepository, which exposes
// no update/delete method. Do not add one here either.
public class LedgerRepository : ILedgerRepository
{
    private readonly ApplicationDbContext _db;

    public LedgerRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(LedgerEntry entry, CancellationToken ct = default)
    {
        await _db.LedgerEntries.AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct = default)
    {
        await _db.LedgerEntries.AddRangeAsync(entries, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.LedgerEntries
            .Where(x => x.VendorProfileId == vendorProfileId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LedgerEntry>> GetUnbatchedByVendorIdAsync(Guid vendorProfileId, DateTime periodEnd, CancellationToken ct = default)
        => await _db.LedgerEntries
            .Where(x => x.VendorProfileId == vendorProfileId
                        && x.PayoutBatchId == null
                        && x.CreatedAt <= periodEnd)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
}
