using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

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

    public async Task<(List<DisputeListItem> Items, int TotalCount)> GetPagedAsync(
        PaymentDisputeStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var bookings = _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Listing)
                .ThenInclude(l => l.VendorProfile);

        var query =
            from d in _db.PaymentDisputes
            join p in _db.Payments on d.PaymentId equals p.Id
            join b in bookings on p.BookingId equals b.Id
            select new { d, b };

        if (status.HasValue)
        {
            query = query.Where(x => x.d.Status == status.Value);
        }

        query = query.OrderByDescending(x => x.d.OpenedAt);

        var totalCount = await query.CountAsync(ct);

        var page = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = page.Select(x => new DisputeListItem(
            x.d,
            x.b.BookingNumber,
            $"{x.b.Customer.FirstName} {x.b.Customer.LastName}",
            x.b.Listing.VendorProfile.BusinessName
        )).ToList();

        return (items, totalCount);
    }

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
