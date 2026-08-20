using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface ITrustedDeviceRepository
{
    Task AddAsync(TrustedDevice device);
    Task<TrustedDevice?> GetValidAsync(Guid userId, string tokenHash);
    Task UpdateAsync(TrustedDevice device);
    Task DeleteAllForUserAsync(Guid userId);
}
