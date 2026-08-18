using Ube.Application.Features.Payments;

namespace Ube.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<VendorDashboardDto> GetVendorDashboardAsync(Guid vendorId);
    Task<VendorBookingCountsDto> GetVendorBookingCountsAsync(Guid vendorId);
    Task<VendorEarningsDto> GetVendorEarningsAsync(Guid vendorUserId, Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default);
}