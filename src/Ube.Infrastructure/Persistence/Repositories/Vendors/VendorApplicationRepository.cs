using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Models;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;

public class VendorApplicationRepository : IVendorApplicationRepository
{
    private readonly ApplicationDbContext _db;
    public VendorApplicationRepository(ApplicationDbContext db)    {
        _db = db;
    }

    public async Task<VendorApplication?> GetByIdAsync(Guid id)
    {
        return await _db.VendorApplications.FindAsync(id);
    }

    public async Task UpdateAsync(VendorApplication application)
    {
        _db.VendorApplications.Update(application);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<VendorApplication> Items, int TotalItems)> GetPagedAsync(VendorApplicationStatus? status, QueryOptions options)
    {
        var query = _db.VendorApplications.AsQueryable();
        // Filter by status if provided
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        // Apply search filter if provided
        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(a => a.BusinessName.Contains(options.Search));
        }
        if(!string.IsNullOrEmpty(options.SortBy))
        {
            // Apply sorting based on the SortBy parameter
            query = options.SortBy switch
            {
                "businessName" => options.IsDescending ? query.OrderByDescending(a => a.BusinessName) : query.OrderBy(a => a.BusinessName),
                "submittedAt" => options.IsDescending ? query.OrderByDescending(a => a.SubmittedAt) : query.OrderBy(a => a.SubmittedAt),
                _ => query // No sorting if SortBy is not recognized
            };
        }
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();
        return (items, totalItems);
    }
}