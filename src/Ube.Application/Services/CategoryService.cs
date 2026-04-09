using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Category;
using Ube.Application.Interfaces;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IAppDbContext _context;

        public CategoryService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    ListingCount = c.Listings.Count()
                })
                .ToListAsync();
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync(CancellationToken.None);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                ListingCount = 0
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.Name = dto.Name;
            category.IsActive = dto.IsActive;

            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = !category.IsActive;

            await _context.SaveChangesAsync(CancellationToken.None);
            return true;
        }
    }
}