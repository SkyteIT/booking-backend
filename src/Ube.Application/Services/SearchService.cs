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
            // Only return Active listings whose category is also Active.
            // Inactive/Deleted listings (e.g. user-created ones with default Status=0)
            // are intentionally excluded.
            .Where(x => x.Status == RecordStatus.Active
                     && x.Category.Status == RecordStatus.Active);

        if (request.CategoryIds.Count > 0)
            query = query.Where(x => request.CategoryIds.Contains(x.CategoryId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // EF Core translates this to SQL LIKE which is case-insensitive on most
            // SQL Server collations (CI). We also normalise to lower on both sides as
            // a defensive measure for CS collations.
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Category.Name.ToLower().Contains(term) ||
                x.Location.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            var loc = request.Location.Trim().ToLower();
            query = query.Where(x => x.Location.ToLower().Contains(loc));
        }

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