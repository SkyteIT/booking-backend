using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Content;
using Ube.Domain.Entities.Content;

namespace Ube.Infrastructure.Persistence.Repositories.Content;

public class PromotionRepository : IPromotionRepository
{
    private readonly ApplicationDbContext _db;

    public PromotionRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken ct = default)
        => await _db.Promotions.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Promotions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
        => await _db.Promotions.AddAsync(promotion, ct);

    public Task DeleteAsync(Promotion promotion, CancellationToken ct = default)
    {
        _db.Promotions.Remove(promotion);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
