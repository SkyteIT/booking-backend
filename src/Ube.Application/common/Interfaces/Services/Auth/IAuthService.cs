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
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId);
}