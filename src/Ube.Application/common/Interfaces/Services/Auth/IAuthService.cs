using Ube.Application.Features.Auth;

namespace Ube.Application.Common.Interfaces.Services.Auth;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task VerifyEmailAsync(string token);
}