using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Models;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;

public class VendorApplicationRepository : IVendorApplicationRepository
{
    private readonly ApplicationDbContext _db;

    public VendorApplicationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<VendorApplication?> GetByIdAsync(Guid id)
        => await _db.VendorApplications.FindAsync(id);

    public async Task AddAsync(VendorApplication application)
    {
        _db.VendorApplications.Add(application);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(VendorApplication application)
    {
        _db.VendorApplications.Update(application);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<ApplicationTableDto> Items, int TotalItems)> GetPagedTableAsync(
        VendorApplicationStatus? status, VendorApplicationsRequest options)
    {
        var query = _db.VendorApplications.AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (!string.IsNullOrEmpty(options.Search))
            query = query.Where(a => a.BusinessName.Contains(options.Search));

        query = options.SortOptions switch
        {
            VendorApplicationSortBy.BusinessNameAsc => query.OrderBy(a => a.BusinessName),
            VendorApplicationSortBy.BusinessNameDesc => query.OrderByDescending(a => a.BusinessName),
            VendorApplicationSortBy.SubmittedAtAsc => query.OrderBy(a => a.SubmittedAt),
            VendorApplicationSortBy.SubmittedAtDesc => query.OrderByDescending(a => a.SubmittedAt),
            VendorApplicationSortBy.Oldest => query.OrderBy(a => a.SubmittedAt),
            VendorApplicationSortBy.Newest => query.OrderByDescending(a => a.SubmittedAt),
            _ => query.OrderByDescending(a => a.SubmittedAt)
        };

        var totalItems = await query.CountAsync();

        var items = await query
            .Select(a => new ApplicationTableDto
            {
                Id = a.Id,
                ApplicantName = a.FirstName + " " + a.LastName,
                Email = a.Email,
                Phone = a.Phone,
                BusinessName = a.BusinessName,
                BusinessType = a.BusinessType,
                Status = a.Status,
                SubmittedAt = a.SubmittedAt
            })
            .Skip((options.PageNumber - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
