using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Infrastructure.Persistence.Repositories.Users;

public class RoleChangeRequestRepository : IRoleChangeRequestRepository
{
    private readonly ApplicationDbContext _db;

    public RoleChangeRequestRepository(ApplicationDbContext db) => _db = db;

    public async Task<RoleChangeRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.RoleChangeRequests
            .Include(x => x.TargetUser)
            .Include(x => x.RequestedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<RoleChangeRequest>> GetByRequesterAsync(Guid requestedByUserId, CancellationToken ct = default)
        => await _db.RoleChangeRequests
            .Include(x => x.TargetUser)
            .Include(x => x.RequestedByUser)
            .Where(x => x.RequestedByUserId == requestedByUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<(List<RoleChangeRequest> Items, int TotalCount)> GetPagedAsync(
        RoleChangeRequestStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.RoleChangeRequests
            .Include(x => x.TargetUser)
            .Include(x => x.RequestedByUser)
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

    public async Task AddAsync(RoleChangeRequest request, CancellationToken ct = default)
    {
        await _db.RoleChangeRequests.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RoleChangeRequest request, CancellationToken ct = default)
    {
        _db.RoleChangeRequests.Update(request);
        await _db.SaveChangesAsync(ct);
    }
}
