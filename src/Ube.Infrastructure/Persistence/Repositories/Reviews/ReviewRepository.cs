using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Reviews;
using Ube.Application.Common.Models;
using Ube.Domain.Entities.Reviews;

namespace Ube.Infrastructure.Persistence.Repositories.Reviews;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public ReviewRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // Add review
    public async Task AddAsync(Review review)
    {
        await _db.Reviews.AddAsync(review);
    }

    // Check if review exists for booking
    public async Task<bool> ExistsByBookingIdAsync(Guid bookingId)
    {
        return await _db.Reviews
            .AnyAsync(x => x.BookingId == bookingId);
    }

    // A booking has at most one review - used to surface the review (and
    // any vendor reply) directly on the booking detail view.
    public async Task<Review?> GetByBookingIdAsync(Guid bookingId)
    {
        return await _db.Reviews.FirstOrDefaultAsync(x => x.BookingId == bookingId);
    }

    // Get reviews by vendor with QueryOptions
    public async Task<(List<Review> Items, int TotalCount)> GetPagedByVendorAsync(
        Guid vendorId,
        QueryOptions options)
    {
        var query = _db.Reviews
            .Include(x => x.Customer)
            .Where(x => x.VendorId == vendorId && !x.IsHidden)
            .AsQueryable();

        // Search (by comment)
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x =>
                x.Comment.Contains(options.Search.ToLower()));
        }

        // Default sorting (latest first)
        query = query.OrderByDescending(x => x.CreatedAt);

        // Total count
        var totalCount = await query.CountAsync();

        //  Pagination
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Get reviews for a single listing with QueryOptions
    public async Task<(List<Review> Items, int TotalCount)> GetPagedByListingAsync(
        Guid listingId,
        QueryOptions options)
    {
        var query = _db.Reviews
            .Include(x => x.Customer)
            .Where(x => x.ListingId == listingId && !x.IsHidden)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x =>
                x.Comment.Contains(options.Search.ToLower()));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
    // Get a customer's own reviews (their "My Reviews" page) - includes hidden
    // reviews too, since it's the customer's own content and they should be
    // able to see/manage it regardless of admin moderation state.
    public async Task<(List<Review> Items, int TotalCount)> GetPagedByCustomerAsync(
        Guid customerId,
        QueryOptions options)
    {
        var query = _db.Reviews
            .Include(x => x.Listing)
            .Where(x => x.CustomerId == customerId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x =>
                x.Comment.Contains(options.Search.ToLower()));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(double AverageRating, int TotalCount)> GetRatingAsync(Guid vendorId)
    {
        var query = _db.Reviews
            .Where(x => x.VendorId == vendorId && !x.IsHidden);
        var count = await query.CountAsync();
        if (count == 0) return (0, 0);
        var average = await query.AverageAsync(x => x.Rating);
        return (average, count);
    }

    // Admin moderation queue - sees hidden reviews too, optionally filtered by hidden status
    public async Task<(List<Review> Items, int TotalCount)> GetPagedForModerationAsync(
        bool? isHidden,
        QueryOptions options)
    {
        var query = _db.Reviews
            .Include(x => x.Customer)
            .AsQueryable();

        if (isHidden.HasValue)
        {
            query = query.Where(x => x.IsHidden == isHidden.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x =>
                x.Comment.Contains(options.Search.ToLower()));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        return await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
    }
    public async Task UpdateAsync(Review review)
    {
        _db.Reviews.Update(review);
        await _db.SaveChangesAsync();
    }
    public async Task DeleteAsync(Review review)
    {
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    // Grouped count per review id for one page of reviews - avoids an
    // N+1 count query per card.
    public async Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> reviewIds)
    {
        var ids = reviewIds.ToList();
        if (ids.Count == 0) return new Dictionary<Guid, int>();

        return await _db.ReviewLikes
            .Where(l => ids.Contains(l.ReviewId))
            .GroupBy(l => l.ReviewId)
            .Select(g => new { ReviewId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ReviewId, x => x.Count);
    }

    // Which of these reviews the given customer has already liked - for
    // rendering the "liked by you" state on a page of reviews.
    public async Task<HashSet<Guid>> GetLikedReviewIdsAsync(Guid customerId, IEnumerable<Guid> reviewIds)
    {
        var ids = reviewIds.ToList();
        if (ids.Count == 0) return new HashSet<Guid>();

        var liked = await _db.ReviewLikes
            .Where(l => l.CustomerId == customerId && ids.Contains(l.ReviewId))
            .Select(l => l.ReviewId)
            .ToListAsync();

        return liked.ToHashSet();
    }

    public async Task<bool> ToggleLikeAsync(Guid reviewId, Guid customerId)
    {
        var existing = await _db.ReviewLikes
            .FirstOrDefaultAsync(l => l.ReviewId == reviewId && l.CustomerId == customerId);

        if (existing != null)
        {
            _db.ReviewLikes.Remove(existing);
            await _db.SaveChangesAsync();
            return false;
        }

        await _db.ReviewLikes.AddAsync(new Domain.Entities.Reviews.ReviewLike
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            CustomerId = customerId
        });
        await _db.SaveChangesAsync();
        return true;
    }
}