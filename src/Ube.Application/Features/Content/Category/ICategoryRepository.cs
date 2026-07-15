using Ube.Domain.Enums;

namespace Ube.Application.Features.Content.Category;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Ube.Domain.Entities.Listings.Category>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Ube.Domain.Entities.Listings.Category>> GetFilteredAsync(RecordStatus? status, string? search, CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetByIdAsync(Guid id, bool includeListings = false, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetDeletedByNameAsync(string name, CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetUncategorizedAsync(CancellationToken ct = default);
    Task<int> CountListingsAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Ube.Domain.Entities.Listings.Category category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
