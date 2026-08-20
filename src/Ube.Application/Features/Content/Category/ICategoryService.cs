
namespace Ube.Application.Features.Content.Category;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken);
    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken);

    /// <summary>Hard-deletes the category and all its listings from the database.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Toggle active/inactive status.</summary>
    Task<CategoryDto?> ToggleStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
