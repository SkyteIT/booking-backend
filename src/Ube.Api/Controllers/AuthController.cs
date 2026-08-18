using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request.IdToken);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { message = "Email verified successfully" });
    }

    // Always returns the same generic message, regardless of whether the
    // email is registered - prevents an attacker from using this endpoint
    // to enumerate valid accounts.
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.RequestPasswordResetAsync(request.Email);
        return Ok(new { message = "If that email is registered, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok(new { message = "Password reset successful. Please log in with your new password." });
    }

    [HttpPost("2fa/enroll/start")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TwoFactorEnrollmentStartDto>> StartTwoFactorEnrollment([FromBody] TwoFactorChallengeRequestDto request)
    {
        var result = await _authService.StartTwoFactorEnrollmentAsync(request.ChallengeToken);
        return Ok(result);
    }

    [HttpPost("2fa/enroll/confirm")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TwoFactorEnrollmentResultDto>> ConfirmTwoFactorEnrollment([FromBody] TwoFactorVerifyRequestDto request)
    {
        var result = await _authService.ConfirmTwoFactorEnrollmentAsync(request.ChallengeToken, request.Code);
        return Ok(result);
    }

    // Self-service opt-in 2FA - for roles where it isn't mandatory, driven
    // by the current authenticated session rather than a login challenge.
    [HttpPost("2fa/self-enroll/start")]
    [Authorize]
    public async Task<ActionResult<TwoFactorEnrollmentStartDto>> StartSelfServiceTwoFactorEnrollment()
    {
        var result = await _authService.StartSelfServiceTwoFactorEnrollmentAsync(_currentUserService.UserId);
        return Ok(result);
    }

    [HttpPost("2fa/self-enroll/confirm")]
    [Authorize]
    public async Task<ActionResult<List<string>>> ConfirmSelfServiceTwoFactorEnrollment([FromBody] ConfirmSelfServiceTwoFactorDto request)
    {
        var backupCodes = await _authService.ConfirmSelfServiceTwoFactorEnrollmentAsync(_currentUserService.UserId, request.Code);
        return Ok(backupCodes);
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorDto request)
    {
        await _authService.DisableTwoFactorAsync(_currentUserService.UserId, request.CurrentPassword);
        return Ok(new { message = "Two-factor authentication disabled" });
    }

    [HttpPost("2fa/verify")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> VerifyTwoFactorCode([FromBody] TwoFactorVerifyRequestDto request)
    {
        var result = await _authService.VerifyTwoFactorCodeAsync(request.ChallengeToken, request.Code);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("current-user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync(_currentUserService.UserId);
        if (user == null) return NotFound();

        return Ok(user);
    }
}