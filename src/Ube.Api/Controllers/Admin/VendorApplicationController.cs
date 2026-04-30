using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Admin.VendorApplications;
using Ube.Application.Features.Vendors;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Domain.Enums.Vendors;
using Ube.Application.Common.Models;

namespace Ube.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/vendor-applications")]
public class VendorApplicationsController : ControllerBase
{
    private readonly IAdminVendorApplicationService _service;
    private readonly ICurrentUserService _currentUser;

    public VendorApplicationsController(
        IAdminVendorApplicationService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    //   (Approve / Reject)
    [HttpPatch("{applicationId}/review")]
    public async Task<IActionResult> ReviewApplication(
        Guid applicationId,
        [FromBody] ReviewVendorApplicationDto dto)
    {
        var userId = _currentUser.UserId;
        await _service.ReviewApplicationAsync(
            applicationId,
            userId, // adminId from JWT
            dto
        );

        return Ok(new { message = "Application reviewed successfully" });
    }
    [HttpGet("{applicationId}")]
    public async Task<IActionResult> GetDetails(Guid applicationId)
    {
        var result = await _service.GetDetailsAsync(applicationId);
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] VendorApplicationStatus? status,
        [FromQuery] QueryOptions request)
    {
        var result = await _service.GetAllAsync(status, request);
        return Ok(result);
    }
}