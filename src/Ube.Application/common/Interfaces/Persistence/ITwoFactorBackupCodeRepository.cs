using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface ITwoFactorBackupCodeRepository
{
    Task AddRangeAsync(IEnumerable<TwoFactorBackupCode> codes);
    Task<List<TwoFactorBackupCode>> GetUnusedByUserIdAsync(Guid userId);
    Task UpdateAsync(TwoFactorBackupCode code);
    Task DeleteAllForUserAsync(Guid userId);
}
