using Ube.Application.Features.Auth;

namespace Ube.Application.Common.Interfaces.Services.Auth;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> GoogleLoginAsync(string idToken);
    Task VerifyEmailAsync(string token);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task<TwoFactorEnrollmentStartDto> StartTwoFactorEnrollmentAsync(string challengeToken);
    Task<TwoFactorEnrollmentResultDto> ConfirmTwoFactorEnrollmentAsync(string challengeToken, string code);
    Task<AuthResponseDto> VerifyTwoFactorCodeAsync(string challengeToken, string code);

    // Self-service variants - for a role where 2FA isn't mandatory, a user
    // opts in from within an already-authenticated session (Settings), so
    // these work off the current user's id instead of a login challenge.
    Task<TwoFactorEnrollmentStartDto> StartSelfServiceTwoFactorEnrollmentAsync(Guid userId);
    Task<List<string>> ConfirmSelfServiceTwoFactorEnrollmentAsync(Guid userId, string code);
    Task DisableTwoFactorAsync(Guid userId, string currentPassword);

    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId);
    Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<CurrentUserDto> UpdateProfileImageAsync(Guid userId, string imageUrl);
}