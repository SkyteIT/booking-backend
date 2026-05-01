using Ube.Application.Features.Auth;

namespace Ube.Application.Common.Interfaces.Services.Auth;
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}