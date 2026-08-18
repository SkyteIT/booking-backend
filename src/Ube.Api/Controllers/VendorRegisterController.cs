using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vendor-register")]
public class VendorRegisterController : ControllerBase
{
    private readonly IVendorApplicationSubmissionService _submissionService;
    private readonly ICurrentUserService _currentUser;

    public VendorRegisterController(
        IVendorApplicationSubmissionService submissionService,
        ICurrentUserService currentUser)
    {
        _submissionService = submissionService;
        _currentUser = currentUser;
    }

    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Submit([FromForm] SubmitVendorApplicationRequest request, CancellationToken cancellationToken)
    {
        var applicationId = await _submissionService.SubmitAsync(_currentUser.UserId, request, cancellationToken);
        return Ok(new
        {
            applicationId,
            message = "Vendor application submitted successfully"
        });
    }
}
