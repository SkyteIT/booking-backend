using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Listings;

namespace Ube.Infrastructure.Persistence.Repositories.Listings;

public class ListingOfferRepository : IListingOfferRepository
{
    private readonly ApplicationDbContext _db;

    public ListingOfferRepository(ApplicationDbContext db) => _db = db;

    public async Task<ListingOffer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.ListingOffers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<ListingOffer>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default)
        => await _db.ListingOffers
            .Where(x => x.ListingId == listingId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<ListingOffer?> GetActiveDiscountForListingAsync(Guid listingId, DateOnly today, CancellationToken ct = default)
        => await _db.ListingOffers
            .FirstOrDefaultAsync(x =>
                x.ListingId == listingId &&
                x.IsActive &&
                x.DiscountType != null &&
                x.StartDate <= today &&
                x.EndDate >= today,
                ct);

    public async Task<bool> HasActiveDiscountOverlapAsync(Guid listingId, DateOnly start, DateOnly end, Guid? excludeOfferId, CancellationToken ct = default)
        => await _db.ListingOffers.AnyAsync(x =>
            x.ListingId == listingId &&
            x.IsActive &&
            x.DiscountType != null &&
            (excludeOfferId == null || x.Id != excludeOfferId) &&
            x.StartDate <= end &&
            x.EndDate >= start,
            ct);

    public async Task AddAsync(ListingOffer offer, CancellationToken ct = default)
    {
        await _db.ListingOffers.AddAsync(offer, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ListingOffer offer, CancellationToken ct = default)
    {
        _db.ListingOffers.Update(offer);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ListingOffer offer, CancellationToken ct = default)
    {
        _db.ListingOffers.Remove(offer);
        await _db.SaveChangesAsync(ct);
    }
}
