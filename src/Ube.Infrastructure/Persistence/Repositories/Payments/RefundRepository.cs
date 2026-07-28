using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class RefundRepository : IRefundRepository
{
    private readonly ApplicationDbContext _db;

    public RefundRepository(ApplicationDbContext db) => _db = db;

    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Refunds.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default)
        => await _db.Refunds.Where(x => x.PaymentId == paymentId).ToListAsync(ct);

    public async Task AddAsync(Refund refund, CancellationToken ct = default)
    {
        await _db.Refunds.AddAsync(refund, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Refund refund, CancellationToken ct = default)
    {
        _db.Refunds.Update(refund);
        await _db.SaveChangesAsync(ct);
    }
}
