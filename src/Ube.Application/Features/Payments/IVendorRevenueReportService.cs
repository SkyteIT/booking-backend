namespace Ube.Application.Features.Payments;

public interface IVendorRevenueReportService
{
    Task<List<RevenuePeriodDto>> GetReportAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default);
    Task<byte[]> ExportReportCsvAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default);
    Task<GrossCommissionRefundBreakdownDto> GetGrossCommissionRefundBreakdownAsync(Guid vendorProfileId, RevenueReportRequest request, CancellationToken ct = default);
}
