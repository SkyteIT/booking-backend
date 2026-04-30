using Ube.Application.Features.Vendors;
namespace Ube.Application.Features.Admin.VendorApplications;
public interface IAdminVendorApplicationService
{
    Task ReviewApplicationAsync(Guid applicationId, Guid adminId, ReviewVendorApplicationDto reviewDto);
    Task<ApplicationDetailDto> GetDetailsAsync(Guid applicationId);
    Task <List<ApplicationDetailDto>> GetAllAsync();
}