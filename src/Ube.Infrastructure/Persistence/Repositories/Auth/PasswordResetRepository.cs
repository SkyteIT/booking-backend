using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Infrastructure.Persistence;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly ApplicationDbContext _db;

    public PasswordResetRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _db.PasswordResetTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _db.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _db.PasswordResetTokens.Update(token);
        await _db.SaveChangesAsync();
    }
}
