
namespace Ube.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId);
}