using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PaymentDisputeRepository : IPaymentDisputeRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentDisputeRepository(ApplicationDbContext db) => _db = db;

    public async Task<PaymentDispute?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.PaymentDisputes.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<PaymentDispute>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default)
        => await _db.PaymentDisputes
            .Where(x => x.PaymentId == paymentId)
            .OrderByDescending(x => x.OpenedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PaymentDispute>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await (
            from d in _db.PaymentDisputes
            join p in _db.Payments on d.PaymentId equals p.Id
            where p.VendorProfileId == vendorProfileId
            orderby d.OpenedAt descending
            select d
        ).ToListAsync(ct);

    public async Task AddAsync(PaymentDispute dispute, CancellationToken ct = default)
    {
        await _db.PaymentDisputes.AddAsync(dispute, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PaymentDispute dispute, CancellationToken ct = default)
    {
        _db.PaymentDisputes.Update(dispute);
        await _db.SaveChangesAsync(ct);
    }
}
