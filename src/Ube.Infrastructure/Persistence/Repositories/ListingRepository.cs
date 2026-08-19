using Microsoft.EntityFrameworkCore;
using Ube.Application.Interfaces.Repositories;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Vendors;

namespace Ube.Infrastructure.Persistence.Repositories;

public sealed class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _context;

    public ListingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        return QueryActiveListings()
            .ToListAsync(cancellationToken);
    }

    public Task<Listing?> GetActiveListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return QueryActiveListings()
            .SingleOrDefaultAsync(listing => listing.Id == listingId, cancellationToken);
    }

    public Task<List<Listing>> GetVendorListingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return QueryListings()
            .Where(listing => listing.VendorProfile.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public Task<Listing?> GetOwnedListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        return QueryListings()
            .Where(listing => listing.VendorProfile.UserId == userId)
            .SingleOrDefaultAsync(listing => listing.Id == listingId, cancellationToken);
    }

    public Task<VendorProfile?> GetVendorProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.VendorProfiles.SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return _context.Categories.AnyAsync(category => category.Id == categoryId && category.IsActive, cancellationToken);
    }

    public async Task AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        await _context.Listings.AddAsync(listing, cancellationToken);
    }

    public Task DeleteAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _context.Listings.Remove(listing);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _context.Listings.Update(listing);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Listing> QueryActiveListings()
    {
        return QueryListings()
            .Where(listing => listing.IsActive);
    }

    private IQueryable<Listing> QueryListings()
    {
        return _context.Listings
            .AsNoTracking()
            .Include(listing => listing.Category)
            .Include(listing => listing.VendorProfile)
            .Include(listing => listing.Reviews);
    }
}