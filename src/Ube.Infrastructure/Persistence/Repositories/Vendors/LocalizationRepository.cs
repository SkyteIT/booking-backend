using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Localization;
using Ube.Domain.Entities.Users;

namespace Ube.Infrastructure.Persistence.Repositories.Vendors;
public class LocalizationRepository : ILocalizationRepository
{
    private readonly ApplicationDbContext _db;
    public LocalizationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserLocalizationSettings?> GetByUserIdAsync(Guid userId)
    {
        return await _db.UserLocalizationSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
    public async Task AddAsync(UserLocalizationSettings settings)
    {
        await _db.UserLocalizationSettings.AddAsync(settings);
        await _db.SaveChangesAsync();
    }
    public async Task UpdateAsync(UserLocalizationSettings settings)
    {
        _db.UserLocalizationSettings.Update(settings);
        await _db.SaveChangesAsync();
    }
}