using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Content;
using Ube.Domain.Constants;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;

namespace Ube.Infrastructure.Persistence.Repositories.Content;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => await _db.Categories
            .Where(x => x.Status != RecordStatus.Deleted && x.Name != CategoryConstants.UncategorizedName)
            .Include(x => x.Listings)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetFilteredAsync(RecordStatus? status, string? search, CancellationToken ct = default)
    {
        var query = _db.Categories
            .Where(x => x.Status != RecordStatus.Deleted && x.Name != CategoryConstants.UncategorizedName)
            .Include(x => x.Listings)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(lower));
        }

        return await query.OrderBy(x => x.DisplayOrder).ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(Guid id, bool includeListings = false, CancellationToken ct = default)
    {
        var query = _db.Categories.Where(x => x.Id == id && x.Status != RecordStatus.Deleted);
        if (includeListings) query = query.Include(x => x.Listings);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(
            x => x.Name.ToLower() == name.ToLower() && x.Status != RecordStatus.Deleted, ct);

    public async Task<Category?> GetDeletedByNameAsync(string name, CancellationToken ct = default)
        => await _db.Categories
            .Include(x => x.Listings)
            .FirstOrDefaultAsync(
                x => x.Name.ToLower() == name.ToLower() && x.Status == RecordStatus.Deleted, ct);

    public async Task<Category?> GetUncategorizedAsync(CancellationToken ct = default)
        => await _db.Categories.FirstOrDefaultAsync(x => x.Name == CategoryConstants.UncategorizedName, ct);

    public async Task<int> CountListingsAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Listings.CountAsync(l => l.CategoryId == categoryId, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default)
        => await _db.Categories.AddAsync(category, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
