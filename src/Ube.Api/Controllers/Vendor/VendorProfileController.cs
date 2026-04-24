using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Features.Vendors;
using Ube.Application.Common.Interfaces.Services.Auth;


namespace Ube.API.Controllers;

[Authorize]
[ApiController]
[Route("api/vendor/profile")]
public class VendorProfileController : ControllerBase
{
    private readonly IVendorProfileService _service;
    private readonly ICurrentUserService _currentUser;
    public VendorProfileController(IVendorProfileService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // get vendor profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = _currentUser.UserId;

            var result = await _service.GetVendorProfileAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    //  update vendor profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateVendorProfileDto dto)
    {
        try
        {
            var userId = _currentUser.UserId;

            var result = await _service.UpdateVendorProfileAsync(userId, dto);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}