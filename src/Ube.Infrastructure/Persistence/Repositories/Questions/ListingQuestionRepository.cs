using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Models;
using Ube.Application.Features.Questions;
using Ube.Domain.Entities.Questions;

namespace Ube.Infrastructure.Persistence.Repositories.Questions;

public class ListingQuestionRepository : IListingQuestionRepository
{
    private readonly ApplicationDbContext _db;

    public ListingQuestionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ListingQuestion question)
    {
        await _db.ListingQuestions.AddAsync(question);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<ListingQuestion> Items, int TotalCount)> GetPagedByListingAsync(Guid listingId, QueryOptions options)
    {
        var query = _db.ListingQuestions
            .Include(x => x.Customer)
            .Where(x => x.ListingId == listingId)
            .AsQueryable();

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<ListingQuestion> Items, int TotalCount)> GetPagedByVendorAsync(Guid vendorId, QueryOptions options)
    {
        var query = _db.ListingQuestions
            .Include(x => x.Customer)
            .Where(x => x.VendorId == vendorId)
            .AsQueryable();

        query = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ListingQuestion?> GetByIdAsync(Guid id)
    {
        return await _db.ListingQuestions.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task UpdateAsync(ListingQuestion question)
    {
        _db.ListingQuestions.Update(question);
        await _db.SaveChangesAsync();
    }
}
