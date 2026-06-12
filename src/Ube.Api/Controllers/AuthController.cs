using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Auth;

namespace Ube.Api.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }


    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }


    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { message = "Email verified successfully" });
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        var userId = _currentUserService.UserId;
        var result = await _authService.RefreshTokenAsync(userId);
        return Ok(result);
    }
 
    [HttpGet("current-user")]
    public IActionResult GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        return Ok(new { userId });
    }
}