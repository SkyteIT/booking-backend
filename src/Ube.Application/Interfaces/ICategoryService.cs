using Ube.Application.DTOs.Category;

namespace Ube.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken);
}
