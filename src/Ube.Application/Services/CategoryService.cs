using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Category;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Content;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;

    public CategoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Icon = x.Icon,
                BannerImageUrl = x.BannerImageUrl,
                DisplayOrder = x.DisplayOrder,
                RequiresAdminApproval = x.RequiresAdminApproval,
                IsFeatured = x.IsFeatured,
                Status = x.Status.ToString(),
                ListingCount = x.Listings.Count
            })
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Icon = x.Icon,
                BannerImageUrl = x.BannerImageUrl,
                DisplayOrder = x.DisplayOrder,
                RequiresAdminApproval = x.RequiresAdminApproval,
                IsFeatured = x.IsFeatured,
                Status = x.Status.ToString(),
                ListingCount = x.Listings.Count
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            BannerImageUrl = dto.BannerImageUrl,
            DisplayOrder = dto.DisplayOrder,
            RequiresAdminApproval = dto.RequiresAdminApproval,
            IsFeatured = dto.IsFeatured,
            Status = RecordStatus.Active
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Icon = entity.Icon,
            BannerImageUrl = entity.BannerImageUrl,
            DisplayOrder = entity.DisplayOrder,
            RequiresAdminApproval = entity.RequiresAdminApproval,
            IsFeatured = entity.IsFeatured,
            Status = entity.Status.ToString(),
            ListingCount = 0
        };
    }
}
