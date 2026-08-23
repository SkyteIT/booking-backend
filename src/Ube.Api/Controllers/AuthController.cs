using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Auth;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService, IFileStorageService fileStorage)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<RegistrationResponseDto>> Register(RegisterRequestDto request)
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
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await _authService.GoogleLoginAsync(request.EffectiveIdToken, request.DeviceToken);
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
        var result = await _authService.ConfirmTwoFactorEnrollmentAsync(request.ChallengeToken, request.Code, request.RememberDevice);
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
        var result = await _authService.VerifyTwoFactorCodeAsync(request.ChallengeToken, request.Code, request.RememberDevice);
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

    // Generic - works for any authenticated role. Vendors have their own
    // richer profile (business name, bio, etc.) via VendorProfileController;
    // this covers the basic name/phone fields every account has.
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await _authService.UpdateProfileAsync(_currentUserService.UserId, dto);
        return Ok(user);
    }

    // Generic - works for any authenticated role. Doesn't take effect until
    // the link sent to the NEW address is clicked (see VerifyEmailAsync) -
    // no admin/SuperAdmin approval involved, since a self-service change
    // to your own account carries no privilege risk once ownership of the
    // new inbox is proven. Contrast with EmailChangeRequestsController,
    // which is the maker-checker flow for staff roles specifically.
    [HttpPost("email/change-request")]
    [Authorize]
    public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
    {
        await _authService.RequestEmailChangeAsync(_currentUserService.UserId, dto.NewEmail);
        return Ok(new { message = "Check your new email address for a link to confirm the change." });
    }

    // Generic - same file validation/storage as VendorProfileController's
    // upload-image, just not restricted to the Vendor role.
    [HttpPost("profile/upload-image")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfileImage([FromForm] UploadImageRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            throw new BusinessRuleException("Invalid file");

        var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedTypes.Contains(extension))
            throw new BusinessRuleException("Only JPG/PNG files are allowed");

        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            throw new BusinessRuleException("Profile picture must not exceed 5MB");

        await using var stream = file.OpenReadStream();
        var imageUrl = await _fileStorage.UploadAsync(stream, extension, file.ContentType, IFileStorageService.ImagesContainer);

        var user = await _authService.UpdateProfileImageAsync(_currentUserService.UserId, imageUrl);
        return Ok(user);
    }
}
