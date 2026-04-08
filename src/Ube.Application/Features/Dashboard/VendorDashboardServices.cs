using Ube.Application.Features.Dashboard;

namespace Ube.Application.Features.Dashboard;
public class VendorDashboardService : IDashboardService
{
    public async Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId)
    {
        return new VendorDashboardDto
    {
        TotalRevenue = 0,
        ActiveBookings = 0,
        TotalListings = 0,
        AverageRating = 0
    };
    }
}