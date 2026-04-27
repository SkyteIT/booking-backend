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
            .Where(x => x.Status == RecordStatus.Active);

        if (request.CategoryIds.Count > 0)
            query = query.Where(x => request.CategoryIds.Contains(x.CategoryId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(x => x.Title.Contains(request.SearchTerm));

        if (!string.IsNullOrWhiteSpace(request.Location))
            query = query.Where(x => x.Location.Contains(request.Location));

        if (request.MinPrice.HasValue)
            query = query.Where(x => x.PriceFrom >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(x => x.PriceFrom <= request.MaxPrice.Value);

        if (request.MinRating.HasValue)
            query = query.Where(x => x.Rating >= request.MinRating.Value);

        if (request.IsAvailable.HasValue)
            query = query.Where(x => x.IsAvailable == request.IsAvailable.Value);

        return await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.PriceFrom)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new SearchListingDto
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.Category.Name,
                Location = x.Location,
                PriceFrom = x.PriceFrom,
                Rating = x.Rating,
                IsFeatured = x.IsFeatured,
                IsAvailable = x.IsAvailable,
                ThumbnailUrl = x.ThumbnailUrl
            })
            .ToListAsync(cancellationToken);
    }
}
