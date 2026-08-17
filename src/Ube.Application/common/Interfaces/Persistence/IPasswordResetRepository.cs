using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IPasswordResetRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task UpdateAsync(PasswordResetToken token);
}