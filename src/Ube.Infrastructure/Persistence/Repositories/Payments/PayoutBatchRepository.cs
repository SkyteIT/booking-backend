using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PayoutBatchRepository : IPayoutBatchRepository
{
    private readonly ApplicationDbContext _db;

    public PayoutBatchRepository(ApplicationDbContext db) => _db = db;

    public async Task<PayoutBatch?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.PayoutBatches.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PayoutBatch>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.PayoutBatches
            .Where(x => x.VendorProfileId == vendorProfileId)
            .OrderByDescending(x => x.PeriodEnd)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PayoutBatch>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await _db.PayoutBatches.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<PayoutBatch>> GetAllPendingAsync(CancellationToken ct = default)
        => await _db.PayoutBatches
            .Where(x => x.Status == PayoutBatchStatus.Pending)
            .OrderBy(x => x.PeriodEnd)
            .ToListAsync(ct);

    public async Task AddAsync(PayoutBatch batch, CancellationToken ct = default)
    {
        await _db.PayoutBatches.AddAsync(batch, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PayoutBatch batch, CancellationToken ct = default)
    {
        _db.PayoutBatches.Update(batch);
        await _db.SaveChangesAsync(ct);
    }
}
