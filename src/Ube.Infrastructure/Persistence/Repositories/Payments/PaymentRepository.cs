using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentRepository(ApplicationDbContext db) => _db = db;

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Payment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await _db.Payments.FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);

    public async Task<IReadOnlyList<Payment>> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
        => await _db.Payments.Where(x => x.BookingId == bookingId).ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        await _db.Payments.AddAsync(payment, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(ct);
    }
}
