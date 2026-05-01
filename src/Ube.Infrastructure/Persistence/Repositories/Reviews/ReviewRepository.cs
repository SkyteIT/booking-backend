using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Models;
using Ube.Domain.Entities.Reviews;

namespace Ube.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public ReviewRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // 🔹 Add review
    public async Task AddAsync(Review review)
    {
        await _db.Reviews.AddAsync(review);
        await _db.SaveChangesAsync();
    }

    // 🔹 Check if review exists for booking
    public async Task<bool> ExistsByBookingIdAsync(Guid bookingId)
    {
        return await _db.Reviews
            .AnyAsync(x => x.BookingId == bookingId);
    }

    // 🔥 Get reviews by vendor with QueryOptions
    public async Task<(List<Review> Items, int TotalCount)> GetPagedByVendorAsync(
        Guid vendorId,
        QueryOptions options)
    {
        var query = _db.Reviews
            .Include(x => x.Customer)
            .Where(x => x.VendorId == vendorId)
            .AsQueryable();

        // 🔹 Search (by comment)
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x =>
                x.Comment.Contains(options.Search));
        }

        // 🔹 Default sorting (latest first)
        query = query.OrderByDescending(x => x.CreatedAt);

        // 🔹 Total count
        var totalCount = await query.CountAsync();

        // 🔹 Pagination
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}