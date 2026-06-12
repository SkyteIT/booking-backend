using Ube.Application.Features.Auth;
using Ube.Domain.Entities.Users;

namespace Ube.Application.Common.Interfaces.Services.Auth;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task VerifyEmailAsync(string token);
    Task<AuthResponseDto> RefreshTokenAsync(Guid userId);
}