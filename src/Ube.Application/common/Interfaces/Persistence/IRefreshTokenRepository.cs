using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAllForUserAsync(Guid userId);
    Task UpdateAsync(RefreshToken token);
}
