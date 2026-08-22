using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Helpers;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Search;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Enums;
using Ube.Domain.Enums.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _db;

    public ListingRepository(ApplicationDbContext db) => _db = db;

    public async Task<Listing?> GetByIdAsync(Guid listingId)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .FirstOrDefaultAsync(l => l.Id == listingId);

    // Batched lookup for callers resolving several listings by id at once
    // (e.g. multi-item checkout) instead of one round trip per id.
    public async Task<List<Listing>> GetByIdsAsync(IEnumerable<Guid> listingIds, CancellationToken ct = default)
        => await _db.Listings
            .Include(l => l.VendorProfile)
            .Where(l => listingIds.Contains(l.Id))
            .ToListAsync(ct);

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

    public async Task<SearchListingsResult> SearchAsync(SearchListingsRequest request, CancellationToken cancellationToken = default)
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

        var today = BusinessDate.Today;

        // Any active offer counts here (perk-only included) - the badge/
        // filter is "does this listing have something running right now,"
        // not specifically "does it have a discount." Checkout's own
        // lookup (GetActiveDiscountForListingAsync) is the one that stays
        // discount-only, since only a discount can affect price.
        if (request.HasActiveOffer == true)
        {
            query = query.Where(x => _db.ListingOffers.Any(o =>
                o.ListingId == x.Id && o.IsActive &&
                o.StartDate <= today && o.EndDate >= today));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.Price)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                Listing = x,
                ActiveOffer = _db.ListingOffers
                    .Where(o => o.ListingId == x.Id && o.IsActive &&
                                o.StartDate <= today && o.EndDate >= today)
                    .Select(o => new { o.DiscountType, o.DiscountValue })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var items = page.Select(r => new SearchListingDto
        {
            Id = r.Listing.Id,
            CategoryId = r.Listing.CategoryId,
            Type = r.Listing.Category.Type ?? r.Listing.Type,
            Title = r.Listing.Title,
            CategoryName = r.Listing.Category.Name,
            Location = r.Listing.Location ?? string.Empty,
            Price = r.Listing.Price,
            AverageRating = r.Listing.AverageRating,
            IsFeatured = r.Listing.IsFeatured,
            IsActive = r.Listing.IsActive,
            ThumbnailUrl = r.Listing.ThumbnailUrl,
            HasActiveOffer = r.ActiveOffer != null,
            OfferBadgeText = r.ActiveOffer == null ? null : FormatOfferBadge(r.ActiveOffer.DiscountType, r.ActiveOffer.DiscountValue)
        }).ToList();

        return new SearchListingsResult { Items = items, TotalCount = totalCount };
    }

    // Called only when an active offer row was actually found (the
    // caller checks HasActiveOffer) - a null DiscountType there means a
    // pure-perk offer, not "no offer at all."
    private static string FormatOfferBadge(OfferDiscountType? type, decimal? value)
        => type switch
        {
            OfferDiscountType.PercentageDiscount => $"{value:0.##}% OFF",
            OfferDiscountType.FixedAmountDiscount => $"{value:0.##} OFF",
            _ => "Special Offer"
        };

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
            .Include(l => l.Units)
            .Include(l => l.Offers)
            .Include(l => l.CustomFieldValues)
                .ThenInclude(v => v.CategoryCustomField)
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
            .Include(l => l.Units)
            .Include(l => l.Offers)
            .Include(l => l.CustomFieldValues)
                .ThenInclude(v => v.CategoryCustomField)
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
            .Include(l => l.Units)
            .Include(l => l.Offers)
            .Include(l => l.CustomFieldValues)
                .ThenInclude(v => v.CategoryCustomField)
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
        // Always set explicitly (not just on the create path) - `details` is
        // a freshly-built object that never had ListingId populated by its
        // caller on the update path, so leaving this to chance previously
        // corrupted the FK to Guid.Empty on every update.
        details.ListingId = listingId;

        var existing = await _db.Set<TDetail>().FirstOrDefaultAsync(d => d.ListingId == listingId, ct);
        if (existing == null)
        {
            _db.Set<TDetail>().Add(details);
        }
        else
        {
            // Deliberately NOT entry.CurrentValues.SetValues(details) - that
            // copies every property matched by name, including the primary
            // key. `details` is a freshly-built object with a default-valued
            // Id (IListingDetail never exposes Id for callers to set), so
            // SetValues throws immediately on the key mismatch ("part of a
            // key and so cannot be modified") before any value even reaches
            // SaveChanges. Instead, copy every scalar property except the key
            // directly via EF metadata.
            var entry = _db.Entry(existing);
            var keyPropertyNames = entry.Metadata.FindPrimaryKey()!.Properties
                .Select(p => p.Name)
                .ToHashSet();

            foreach (var property in entry.Metadata.GetProperties())
            {
                if (keyPropertyNames.Contains(property.Name)) continue;
                entry.Property(property.Name).CurrentValue = property.PropertyInfo!.GetValue(details);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ClearDetailsAsync(Guid listingId, CancellationToken ct = default)
    {
        var hotel = await _db.HotelListingDetails.FirstOrDefaultAsync(x => x.ListingId == listingId, ct);
        var restaurant = await _db.RestaurantListingDetails.FirstOrDefaultAsync(x => x.ListingId == listingId, ct);
        var eventDetails = await _db.EventListingDetails.FirstOrDefaultAsync(x => x.ListingId == listingId, ct);
        var carRental = await _db.CarRentalListingDetails.FirstOrDefaultAsync(x => x.ListingId == listingId, ct);
        var activity = await _db.ActivityListingDetails.FirstOrDefaultAsync(x => x.ListingId == listingId, ct);

        if (hotel != null) _db.HotelListingDetails.Remove(hotel);
        if (restaurant != null) _db.RestaurantListingDetails.Remove(restaurant);
        if (eventDetails != null) _db.EventListingDetails.Remove(eventDetails);
        if (carRental != null) _db.CarRentalListingDetails.Remove(carRental);
        if (activity != null) _db.ActivityListingDetails.Remove(activity);

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceCustomFieldValuesAsync(Guid listingId, IEnumerable<ListingCustomFieldValue> values, CancellationToken ct = default)
    {
        var existing = await _db.ListingCustomFieldValues.Where(v => v.ListingId == listingId).ToListAsync(ct);
        if (existing.Count > 0)
            _db.ListingCustomFieldValues.RemoveRange(existing);

        foreach (var value in values)
        {
            value.ListingId = listingId;
            _db.ListingCustomFieldValues.Add(value);
        }

        await _db.SaveChangesAsync(ct);
    }
}
