using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Infrastructure.Persistence;

public class TwoFactorBackupCodeRepository : ITwoFactorBackupCodeRepository
{
    private readonly ApplicationDbContext _db;

    public TwoFactorBackupCodeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(IEnumerable<TwoFactorBackupCode> codes)
    {
        await _db.TwoFactorBackupCodes.AddRangeAsync(codes);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TwoFactorBackupCode>> GetUnusedByUserIdAsync(Guid userId)
    {
        return await _db.TwoFactorBackupCodes
            .Where(x => x.UserId == userId && !x.IsUsed)
            .ToListAsync();
    }

    public async Task UpdateAsync(TwoFactorBackupCode code)
    {
        _db.TwoFactorBackupCodes.Update(code);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAllForUserAsync(Guid userId)
    {
        var codes = await _db.TwoFactorBackupCodes.Where(x => x.UserId == userId).ToListAsync();
        _db.TwoFactorBackupCodes.RemoveRange(codes);
        await _db.SaveChangesAsync();
    }
}
