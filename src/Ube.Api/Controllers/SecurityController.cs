using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Users;

namespace Ube.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
{
    private readonly SecurityService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    public SecurityController(
        SecurityService service,
        ICurrentUserService currentUser,
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _service = service;
        _currentUser = currentUser;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto)
    {
        var userId = _currentUser.UserId;

        await _service.ChangePasswordAsync(userId, dto);

        return Ok(new { message = "Password updated successfully" });
    }
}