using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Dashboard;
using Ube.Application.Features.Payments;
using Ube.Application.Features.Vendors;
using Ube.Application.Common.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Authorization;
namespace Ube.Api.Controllers.Dashboard;

[Authorize (Roles = "Vendor")]
[ApiController]
[Route("api/vendor/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IVendorRevenueReportService _revenueReportService;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly ICurrentUserService _currentUser;
    public DashboardController(
        IDashboardService dashboardService,
        IVendorRevenueReportService revenueReportService,
        IVendorProfileRepository vendorRepo,
        ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _revenueReportService = revenueReportService;
        _vendorRepo = vendorRepo;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var vendorId = _currentUser.UserId;
        var result = await _dashboardService.GetVendorDashboardAsync(vendorId);
        return Ok(result);
    }

        [HttpGet("counts")]
        public async Task<IActionResult> GetBookingCounts()
        {
            var vendorId = _currentUser.UserId;
            var result = await _dashboardService.GetVendorBookingCountsAsync(vendorId);
            return Ok(result);
        }

    // Daily/weekly/monthly net-revenue breakdown, sourced from the vendor's own ledger 
    [HttpGet("revenue-report")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportRequest request, CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var result = await _revenueReportService.GetReportAsync(vendorProfileId, request, ct);
        return Ok(result);
    }

    [HttpGet("revenue-report/export")]
    public async Task<IActionResult> ExportRevenueReport([FromQuery] RevenueReportRequest request, CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var csvBytes = await _revenueReportService.ExportReportCsvAsync(vendorProfileId, request, ct);
        return File(csvBytes, "text/csv", $"revenue-report-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    // Backs the vendor "Earnings" tab: gross/commission/refund breakdown,

    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings([FromQuery] RevenueReportRequest request, CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var result = await _dashboardService.GetVendorEarningsAsync(_currentUser.UserId, vendorProfileId, request, ct);
        return Ok(result);
    }

    // The ledger is keyed by VendorProfileId, not the vendor's raw UserId
    // the rest of this controller uses - same resolution pattern
    // VendorLedgerController already uses for every ledger-backed endpoint.
    private async Task<Guid> ResolveVendorProfileIdAsync()
    {
        var profile = await _vendorRepo.GetVendorIdAsync(_currentUser.UserId)
            ?? throw new NotFoundException("Vendor profile not found");
        return profile.Id;
    }
}
