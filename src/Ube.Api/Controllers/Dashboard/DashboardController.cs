using Ube.Application.Features.Dashboard.DTOs;
using Microsoft.AspNetCore.Mvc;
namespace Ube.Api.Controllers.Dashboard;

[ApiController]
[Route("api/vendor/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid vendorId)
    {
        var result = await _dashboardService.GetVendorDashboardAsync(vendorId);
        return Ok(result);
    }
}