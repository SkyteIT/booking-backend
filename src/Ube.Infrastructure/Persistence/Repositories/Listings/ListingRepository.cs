using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.DTOs.Search;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _db;

    public ListingRepository(ApplicationDbContext db) => _db = db;

    public async Task<Listing?> GetByIdAsync(Guid listingId)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .FirstOrDefaultAsync(l => l.Id == listingId);

    public async Task<List<Listing>> GetByVendorIdAsync(Guid vendorId)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .Where(l => l.VendorProfile.UserId == vendorId)
            .ToListAsync();

    public async Task UpdateAsync(Listing listing)
    {
        _db.Listings.Update(listing);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Listing>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Listings
            .Where(l => l.CategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<List<Listing>> GetOrphanedByCategoryNameAsync(Guid uncategorizedId, string originalName, CancellationToken ct = default)
        => await _db.Listings
            .Where(l => l.CategoryId == uncategorizedId
                     && l.OriginalCategoryName != null
                     && l.OriginalCategoryName.ToLower() == originalName.ToLower())
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SearchListingDto>> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _db.Listings
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

    public async Task<Listing?> GetByIdWithDetailsAsync(Guid listingId, CancellationToken ct = default)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.HotelDetails)
            .Include(l => l.RestaurantDetails)
            .Include(l => l.EventDetails)
            .Include(l => l.CarRentalDetails)
            .Include(l => l.ActivityDetails)
            .FirstOrDefaultAsync(l => l.Id == listingId, ct);

    public async Task<List<Listing>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.HotelDetails)
            .Include(l => l.RestaurantDetails)
            .Include(l => l.EventDetails)
            .Include(l => l.CarRentalDetails)
            .Include(l => l.ActivityDetails)
            .ToListAsync(ct);

    public async Task<List<Listing>> GetByVendorProfileIdWithDetailsAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.Listings
            .Where(l => l.VendorProfileId == vendorProfileId)
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.HotelDetails)
            .Include(l => l.RestaurantDetails)
            .Include(l => l.EventDetails)
            .Include(l => l.CarRentalDetails)
            .Include(l => l.ActivityDetails)
            .ToListAsync(ct);

    public async Task AddAsync(Listing listing, CancellationToken ct = default)
    {
        _db.Listings.Add(listing);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Listing listing, CancellationToken ct = default)
    {
        _db.Listings.Remove(listing);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceImagesAsync(Guid listingId, IEnumerable<string> imageUrls, CancellationToken ct = default)
    {
        var existing = await _db.ListingImages.Where(i => i.ListingId == listingId).ToListAsync(ct);
        if (existing.Count > 0)
            _db.ListingImages.RemoveRange(existing);

        var urls = imageUrls.ToList();
        for (var i = 0; i < urls.Count; i++)
        {
            _db.ListingImages.Add(new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                ImageUrl = urls[i],
                IsPrimary = i == 0
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<TDetail?> GetDetailsAsync<TDetail>(Guid listingId, CancellationToken ct = default)
        where TDetail : class, IListingDetail
        => await _db.Set<TDetail>().FirstOrDefaultAsync(d => d.ListingId == listingId, ct);

    public async Task UpsertDetailsAsync<TDetail>(Guid listingId, TDetail details, CancellationToken ct = default)
        where TDetail : class, IListingDetail
    {
        var existing = await _db.Set<TDetail>().FirstOrDefaultAsync(d => d.ListingId == listingId, ct);
        if (existing == null)
        {
            details.ListingId = listingId;
            _db.Set<TDetail>().Add(details);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(details);
        }

        await _db.SaveChangesAsync(ct);
    }
}
