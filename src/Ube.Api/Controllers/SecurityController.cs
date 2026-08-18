using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Security;

namespace Ube.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _service;
    private readonly ICurrentUserService _currentUser;

    public SecurityController(ISecurityService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _service.ChangePasswordAsync(_currentUser.UserId, dto);
        return Ok(new { message = "Password updated successfully" });
    }
}