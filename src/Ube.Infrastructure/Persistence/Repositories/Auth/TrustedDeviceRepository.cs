using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Infrastructure.Persistence;

public class TrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly ApplicationDbContext _db;

    public TrustedDeviceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(TrustedDevice device)
    {
        await _db.TrustedDevices.AddAsync(device);
        await _db.SaveChangesAsync();
    }

    public async Task<TrustedDevice?> GetValidAsync(Guid userId, string tokenHash)
    {
        return await _db.TrustedDevices
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash && x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task UpdateAsync(TrustedDevice device)
    {
        _db.TrustedDevices.Update(device);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAllForUserAsync(Guid userId)
    {
        var devices = await _db.TrustedDevices.Where(x => x.UserId == userId).ToListAsync();
        _db.TrustedDevices.RemoveRange(devices);
        await _db.SaveChangesAsync();
    }
}
