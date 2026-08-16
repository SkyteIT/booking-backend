using Ube.Application.Features.Auth;

namespace Ube.Application.Common.Interfaces.Services.Auth;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> GoogleLoginAsync(string idToken);
    Task VerifyEmailAsync(string token);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId);
}