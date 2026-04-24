using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Dashboard;
using Ube.Application.Common.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Authorization;
namespace Ube.Api.Controllers.Dashboard;

[Authorize(Roles = "Vendor")]
[ApiController]
[Route("api/vendor/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;
    public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var vendorId = _currentUser.UserId;
        var result = await _dashboardService.GetVendorDashboardAsync(vendorId);
        return Ok(result);
    }
}