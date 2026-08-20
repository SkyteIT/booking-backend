using Ube.Application.Common.Models;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Domain.Entities.Vendors;
using Ube.Domain.Enums.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorApplicationRepository
{
    Task<VendorApplication?> GetByIdAsync(Guid id);

    // The applicant's own most recent submission, self-service status
    // check - null means they've never applied.
    Task<VendorApplication?> GetLatestByUserIdAsync(Guid userId);

    Task AddAsync(VendorApplication application);
    Task UpdateAsync(VendorApplication application);
    Task<(List<ApplicationTableDto> Items, int TotalItems)> GetPagedTableAsync(VendorApplicationStatus? status, VendorApplicationsRequest options);
}