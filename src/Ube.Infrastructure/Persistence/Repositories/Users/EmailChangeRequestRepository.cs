using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Infrastructure.Persistence.Repositories.Users;

public class EmailChangeRequestRepository : IEmailChangeRequestRepository
{
    private readonly ApplicationDbContext _db;

    public EmailChangeRequestRepository(ApplicationDbContext db) => _db = db;

    public async Task<EmailChangeRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.EmailChangeRequests
            .Include(x => x.User)
            .Include(x => x.ReviewedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<EmailChangeRequest>> GetByRequesterAsync(Guid userId, CancellationToken ct = default)
        => await _db.EmailChangeRequests
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<bool> HasPendingForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.EmailChangeRequests
            .AnyAsync(x => x.UserId == userId && x.Status == EmailChangeRequestStatus.Pending, ct);

    public async Task<(List<EmailChangeRequest> Items, int TotalCount)> GetPagedAsync(
        EmailChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.EmailChangeRequests
            .Include(x => x.User)
            .Include(x => x.ReviewedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(EmailChangeRequest request, CancellationToken ct = default)
    {
        await _db.EmailChangeRequests.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(EmailChangeRequest request, CancellationToken ct = default)
    {
        _db.EmailChangeRequests.Update(request);
        await _db.SaveChangesAsync(ct);
    }
}
