using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Infrastructure.Persistence;

public class TwoFactorChallengeRepository : ITwoFactorChallengeRepository
{
    private readonly ApplicationDbContext _db;

    public TwoFactorChallengeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(TwoFactorChallenge challenge)
    {
        await _db.TwoFactorChallenges.AddAsync(challenge);
        await _db.SaveChangesAsync();
    }

    public async Task<TwoFactorChallenge?> GetByTokenAsync(string token)
    {
        return await _db.TwoFactorChallenges
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task UpdateAsync(TwoFactorChallenge challenge)
    {
        _db.TwoFactorChallenges.Update(challenge);
        await _db.SaveChangesAsync();
    }
}
