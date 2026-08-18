using Ube.Domain.Entities.Users;


namespace Ube.Application.Features.Localization;
public class LocalizationService : ILocalizationService
{
    private readonly ILocalizationRepository _repo;

    public LocalizationService(ILocalizationRepository repo)
    {
        _repo = repo;
    }
    //Get localization settings for a user, if not exist create default settings
    public async Task<UserLocalizationSettings?> GetLocalizationAsync(Guid userId)
    {
        var settings = await _repo.GetByUserIdAsync(userId);
        if (settings == null)        {
            settings = new UserLocalizationSettings
            {
                UserId = userId,
                Language = "en",
                TimeZone = "UTC",
                Currency = "USD"
            };
            await _repo.AddAsync(settings);
        }
        return settings;
    }
    //Update localization settings for a user, if not exist create new settings
    public async Task UpdateLocalizationAsync(Guid userId, UpdateLocalizationDto dto)
    {
        var settings = await _repo.GetByUserIdAsync(userId);
        if (settings == null)
        {
            settings = new UserLocalizationSettings
            {
               Id = Guid.NewGuid(),
                UserId = userId,
        
            };
            await _repo.AddAsync(settings);
        }
        else
        {
            settings.Language = dto.Language;
            settings.TimeZone = dto.TimeZone;
            settings.Currency = dto.Currency;
            await _repo.UpdateAsync(settings);
        }
    }
}
