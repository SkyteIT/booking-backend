using Ube.Domain.Entities.Listings;

namespace Ube.Application.Features.Content.Category;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Ube.Domain.Entities.Listings.Category>> GetAllAsync(CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetByIdAsync(Guid id, bool includeListings = false, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetDeletedByNameAsync(string name, CancellationToken ct = default);
    Task<Ube.Domain.Entities.Listings.Category?> GetUncategorizedAsync(CancellationToken ct = default);
    Task<int> CountListingsAsync(Guid categoryId, CancellationToken ct = default);
    Task AddAsync(Ube.Domain.Entities.Listings.Category category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // Syncs a category's custom field definitions to the given set: fields
    // whose Id already exists are updated in place (preserving any listing
    // values recorded against them), fields with no existing Id are
    // inserted as new, and existing fields absent from the incoming set are
    // removed (cascading their recorded listing values, since the field no
    // longer exists to hold a value for).
    Task SyncCustomFieldsAsync(Guid categoryId, IEnumerable<CategoryCustomField> fields, CancellationToken ct = default);
}
