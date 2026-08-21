using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Content.Category;
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
            .Include(x => x.CustomFields)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(Guid id, bool includeListings = false, CancellationToken ct = default)
    {
        IQueryable<Category> query = _db.Categories.Where(x => x.Id == id && x.Status != RecordStatus.Deleted)
            .Include(x => x.CustomFields);
        if (includeListings) query = query.Include(x => x.Listings);
        return await query.FirstOrDefaultAsync(ct);
    }

    // Batched lookup for callers resolving several categories by id at once
    // (e.g. multi-item checkout) instead of one round trip per id.
    public async Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken ct = default)
        => await _db.Categories
            .Where(x => categoryIds.Contains(x.Id) && x.Status != RecordStatus.Deleted)
            .ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(
            x => x.Name.ToLower() == name.ToLower() && x.Status != RecordStatus.Deleted, ct);

    public async Task<Category?> GetDeletedByNameAsync(string name, CancellationToken ct = default)
        => await _db.Categories
            .Include(x => x.Listings)
            .Include(x => x.CustomFields)
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

    public async Task SyncCustomFieldsAsync(Guid categoryId, IEnumerable<CategoryCustomField> fields, CancellationToken ct = default)
    {
        var incoming = fields.ToList();
        var incomingIds = incoming.Select(f => f.Id).ToHashSet();

        var existing = await _db.CategoryCustomFields
            .Where(f => f.CategoryId == categoryId)
            .ToListAsync(ct);

        var toRemove = existing.Where(f => !incomingIds.Contains(f.Id)).ToList();
        if (toRemove.Count > 0)
            _db.CategoryCustomFields.RemoveRange(toRemove);

        var existingById = existing.ToDictionary(f => f.Id);
        foreach (var field in incoming)
        {
            if (existingById.TryGetValue(field.Id, out var current))
            {
                current.Label = field.Label;
                current.FieldType = field.FieldType;
                current.Required = field.Required;
                current.DisplayOrder = field.DisplayOrder;
                current.Options = field.Options;
                current.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                field.CategoryId = categoryId;
                await _db.CategoryCustomFields.AddAsync(field, ct);
            }
        }
    }
}
