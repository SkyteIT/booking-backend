using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PayoutExportSettingsRepository : IPayoutExportSettingsRepository
{
    private readonly ApplicationDbContext _db;

    public PayoutExportSettingsRepository(ApplicationDbContext db) => _db = db;

    public async Task<PayoutExportSettings> GetOrCreateAsync(CancellationToken ct = default)
    {
        var existing = await _db.PayoutExportSettingsRows.FirstOrDefaultAsync(ct);
        if (existing != null)
            return existing;

        var settings = new PayoutExportSettings { Id = Guid.NewGuid() };
        await _db.PayoutExportSettingsRows.AddAsync(settings, ct);
        await _db.SaveChangesAsync(ct);
        return settings;
    }

    public async Task UpdateAsync(PayoutExportSettings settings, CancellationToken ct = default)
    {
        _db.PayoutExportSettingsRows.Update(settings);
        await _db.SaveChangesAsync(ct);
    }
}
