using Ube.Application.Features.Users;

namespace Ube.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string> LoginAsync(LoginRequest request);
    Task<string> GoogleLoginAsync(string token);
}
