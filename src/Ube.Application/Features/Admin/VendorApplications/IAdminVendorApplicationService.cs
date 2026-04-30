using Ube.Application.Common.Models;
using Ube.Application.Common.Models.Pagination;
using Ube.Application.Features.Vendors;
using Ube.Domain.Enums.Vendors;
namespace Ube.Application.Features.Admin.VendorApplications;
public interface IAdminVendorApplicationService
{
    Task ReviewApplicationAsync(Guid applicationId, Guid adminId, ReviewVendorApplicationDto reviewDto);
    Task<ApplicationDetailDto> GetDetailsAsync(Guid applicationId);
    Task <PagedResult<ApplicationDetailDto>> GetAllAsync(VendorApplicationStatus? status, QueryOptions options);
}