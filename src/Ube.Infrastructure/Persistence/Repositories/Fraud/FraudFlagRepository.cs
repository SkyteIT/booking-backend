using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Fraud;
using Ube.Domain.Entities.Fraud;
using Ube.Domain.Enums.Fraud;

namespace Ube.Infrastructure.Persistence.Repositories.Fraud;

public class FraudFlagRepository : IFraudFlagRepository
{
    private readonly ApplicationDbContext _db;

    public FraudFlagRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(FraudFlag flag, CancellationToken ct = default)
    {
        await _db.FraudFlags.AddAsync(flag, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<FraudFlag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.FraudFlags.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task UpdateAsync(FraudFlag flag, CancellationToken ct = default)
    {
        _db.FraudFlags.Update(flag);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(List<FraudFlagListItem> Items, int TotalCount)> GetPagedAsync(
        FraudFlagStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query =
            from f in _db.FraudFlags
            join b in _db.Bookings.Include(b => b.Listing) on f.BookingId equals b.Id
            join c in _db.Users on f.CustomerId equals c.Id
            select new { f, b, c };

        if (status.HasValue)
        {
            query = query.Where(x => x.f.Status == status.Value);
        }

        query = query.OrderByDescending(x => x.f.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var page = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = page.Select(x => new FraudFlagListItem(
            x.f,
            x.b.BookingNumber,
            x.b.Listing.Title,
            $"{x.c.FirstName} {x.c.LastName}"
        )).ToList();

        return (items, totalCount);
    }
}
