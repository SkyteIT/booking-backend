using Ube.Domain.Entities.Users;

namespace Ube.Application.Common.Interfaces.Services.Auth;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}