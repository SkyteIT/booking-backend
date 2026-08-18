using Ube.Domain.Entities.Users;
namespace Ube.Application.Features.Localization;
public interface ILocalizationService
{
    Task<UserLocalizationSettings?> GetLocalizationAsync(Guid userId);
    Task UpdateLocalizationAsync(Guid userId, UpdateLocalizationDto dto);
}