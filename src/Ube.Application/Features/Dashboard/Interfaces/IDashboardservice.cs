
namespace Ube.Application.Features.Dashboard.DTOs;

public interface IDashboardService
{
    Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId);
}