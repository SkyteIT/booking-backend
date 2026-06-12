using Microsoft.EntityFrameworkCore;
using Ube.Application.DTOs.Search;
using Ube.Application.Interfaces;
using Ube.Domain.Enums;

namespace Ube.Application.Services;

public class SearchService : ISearchService
{
    private readonly IAppDbContext _context;

    public SearchService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken)
    {
        var query = _context.Listings
            .Include(x => x.Category)
            .Where(x => x.IsActive && x.Category.Status == RecordStatus.Active);

        if (request.CategoryIds.Count > 0)
            query = query.Where(x => request.CategoryIds.Contains(x.CategoryId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Category.Name.ToLower().Contains(term) ||
                (x.Location != null && x.Location.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            var loc = request.Location.Trim().ToLower();
            query = query.Where(x => x.Location != null && x.Location.ToLower().Contains(loc));
        }

        if (request.MinPrice.HasValue)
            query = query.Where(x => x.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= request.MaxPrice.Value);

        if (request.MinRating.HasValue)
            query = query.Where(x => x.AverageRating >= (double)request.MinRating.Value);

        if (request.IsAvailable.HasValue)
            query = query.Where(x => x.IsActive == request.IsAvailable.Value);

        return await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.Price)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new SearchListingDto
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.Category.Name,
                Location = x.Location ?? string.Empty,
                Price = x.Price,
                AverageRating = x.AverageRating,
                IsFeatured = x.IsFeatured,
                IsActive = x.IsActive,
                ThumbnailUrl = x.ThumbnailUrl
            })
            .ToListAsync(cancellationToken);
    }
}
