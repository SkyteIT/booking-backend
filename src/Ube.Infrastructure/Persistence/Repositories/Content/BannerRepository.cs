using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Content.Banner;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Content;

namespace Ube.Infrastructure.Persistence.Repositories.Content;

public class BannerRepository : IBannerRepository
{
    private readonly ApplicationDbContext _db;

    public BannerRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Banner>> GetAllAsync(CancellationToken ct = default)
        => await _db.Banners.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task<IReadOnlyList<Banner>> GetActiveByPlacementAsync(
        BannerPlacement placement,
        DateOnly asOfDate,
        CancellationToken ct = default)
        => await _db.Banners
            .AsNoTracking()
            .Where(x =>
                x.Status == RecordStatus.Active &&
                x.Placement == placement &&
                x.StartDate <= asOfDate &&
                x.EndDate >= asOfDate)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<Banner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Banners.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Banner banner, CancellationToken ct = default)
        => await _db.Banners.AddAsync(banner, ct);

    public Task DeleteAsync(Banner banner, CancellationToken ct = default)
    {
        _db.Banners.Remove(banner);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
