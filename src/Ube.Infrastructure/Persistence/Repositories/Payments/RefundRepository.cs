using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class RefundRepository : IRefundRepository
{
    private readonly ApplicationDbContext _db;

    public RefundRepository(ApplicationDbContext db) => _db = db;

    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Refunds.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken ct = default)
        => await _db.Refunds.Where(x => x.PaymentId == paymentId).ToListAsync(ct);

    // Refund has no navigation properties to Payment/Booking/Customer/Listing
    // by design (see IRefundRepository) - this is a plain LINQ join on the
    // foreign-key values, no changes to the domain entities needed.
    public async Task<(List<RefundListItem> Items, int TotalCount)> GetPagedAsync(
        RefundStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var bookings = _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile);

        var query =
            from r in _db.Refunds
            join p in _db.Payments on r.PaymentId equals p.Id
            join b in bookings on p.BookingId equals b.Id
            select new { r, b };

        if (status.HasValue)
        {
            query = query.Where(x => x.r.Status == status.Value);
        }

        query = query.OrderByDescending(x => x.r.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var page = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = page.Select(x => new RefundListItem(
            x.r,
            x.b.BookingNumber,
            $"{x.b.Customer.FirstName} {x.b.Customer.LastName}",
            x.b.Listing.VendorProfile.BusinessName,
            x.b.Listing.Title
        )).ToList();

        return (items, totalCount);
    }

    public async Task<Dictionary<Guid, decimal>> GetProcessedAmountsByPaymentIdsAsync(IEnumerable<Guid> paymentIds, CancellationToken ct = default)
    {
        var ids = paymentIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, decimal>();

        return await _db.Refunds
            .Where(r => ids.Contains(r.PaymentId) && r.Status == RefundStatus.Processed)
            .GroupBy(r => r.PaymentId)
            .Select(g => new { PaymentId = g.Key, Total = g.Sum(r => r.Amount) })
            .ToDictionaryAsync(x => x.PaymentId, x => x.Total, ct);
    }

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
