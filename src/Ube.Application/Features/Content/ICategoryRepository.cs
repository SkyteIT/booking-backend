using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;

namespace Ube.Application.Features.Content;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetFilteredAsync(RecordStatus? status, string? search, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, bool includeListings = false, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<Category?> GetDeletedByNameAsync(string name, CancellationToken ct = default);
    Task<Category?> GetUncategorizedAsync(CancellationToken ct = default);
    Task<int> CountListingsAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
