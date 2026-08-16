using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PaymentAuditLogRepository : IPaymentAuditLogRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentAuditLogRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(PaymentAuditLogEntry entry, CancellationToken ct = default)
    {
        await _db.PaymentAuditLogEntries.AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }
}
