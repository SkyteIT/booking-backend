using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface ITwoFactorChallengeRepository
{
    Task AddAsync(TwoFactorChallenge challenge);
    Task<TwoFactorChallenge?> GetByTokenAsync(string token);
    Task UpdateAsync(TwoFactorChallenge challenge);
}
