using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Bookings;
using Ube.Application.Features.Bookings.Validators;
using Ube.Application.Features.VendorRegistration;

namespace Ube.Api.Controllers.VendorRegistration;

[Authorize]
[ApiController]
[Route("api/vendor-register")]
public class VendorRegisterController : ControllerBase
{
    private readonly IVendorRegistrationService _vendorRegistrationService;
    private readonly ICurrentUserService _currentUser;

    public VendorRegisterController(
        IVendorRegistrationService vendorRegistrationService,
        ICurrentUserService currentUser)
    {
        _vendorRegistrationService = vendorRegistrationService;
        _currentUser = currentUser;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitApplication(
        [FromForm] VendorRegisterApplicationDto dto,
        IFormFile? businessLicense,
        IFormFile? insuranceCertificate,
        IFormFile? taxDocument)
    {
        VendorApplicationDocumentValidator.Validate(businessLicense, "Business license");
        VendorApplicationDocumentValidator.Validate(insuranceCertificate, "Insurance certificate", required: false);
        VendorApplicationDocumentValidator.Validate(taxDocument, "Tax document", required: false);

        var id = await _vendorRegistrationService.SubmitApplicationAsync(
            _currentUser.UserId,
            dto,
            businessLicense?.OpenReadStream(), Path.GetExtension(businessLicense?.FileName),
            insuranceCertificate?.OpenReadStream(), Path.GetExtension(insuranceCertificate?.FileName),
            taxDocument?.OpenReadStream(), Path.GetExtension(taxDocument?.FileName));

        return Ok(new { id });
    }

    // Self-service - lets the applicant check their own application's
    // status without needing an Admin to tell them directly. Null means
    // they've never applied.
    [HttpGet("status")]
    public async Task<IActionResult> GetMyStatus()
    {
        var status = await _vendorRegistrationService.GetMyStatusAsync(_currentUser.UserId);
        return Ok(status);
    }
}
