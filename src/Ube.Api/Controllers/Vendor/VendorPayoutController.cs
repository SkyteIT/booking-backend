using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Vendors.Payout;
using Ube.Application.Common.Interfaces.Services.Auth;

namespace Ube.Api.Controllers.Vendor;

[Authorize]
[ApiController]
[Route("api/vendor/payout")]
public class VendorPayoutController : ControllerBase
{
    private readonly IVendorPayoutService _service;
    private readonly ICurrentUserService _currentUser;

    public VendorPayoutController(
        IVendorPayoutService service,
        ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAsync(_currentUser.UserId);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateVendorPayoutDto dto)
    {
        await _service.UpdateAsync(_currentUser.UserId, dto);
        return Ok("Payout details updated");
    }
}