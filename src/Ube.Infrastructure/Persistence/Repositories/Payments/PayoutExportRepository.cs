using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PayoutExportRepository : IPayoutExportRepository
{
    private readonly ApplicationDbContext _db;

    public PayoutExportRepository(ApplicationDbContext db) => _db = db;

    public async Task<PayoutExportRun?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.PayoutExportRuns.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PayoutExportRun>> GetPendingAsync(CancellationToken ct = default)
        => await _db.PayoutExportRuns
            .Where(x => x.Status == PayoutExportStatus.PendingApproval || x.Status == PayoutExportStatus.PendingSeniorApproval)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(ct);

    public async Task AddAsync(PayoutExportRun run, CancellationToken ct = default)
    {
        await _db.PayoutExportRuns.AddAsync(run, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PayoutExportRun run, CancellationToken ct = default)
    {
        _db.PayoutExportRuns.Update(run);
        await _db.SaveChangesAsync(ct);
    }
}
