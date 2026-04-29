using Ube.Domain.Entities.Users;

namespace Ube.Application.Features.Localization;
public interface ILocalizationRepository
{
    Task<UserLocalizationSettings?> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserLocalizationSettings settings);
    Task UpdateAsync(UserLocalizationSettings settings);
}