using Microsoft.EntityFrameworkCore;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Auth;
using Ube.Infrastructure.Persistence;


public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly ApplicationDbContext _db;

    public EmailVerificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    // this is used when we generate a new token for email verification, we need to save it to the database so we can verify it later when user clicks the link
    public async Task AddAsync(EmailVerificationToken token)
    {
        await _db.EmailVerificationTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }
    //this is used when user clicks the verification link, we need to find the token and mark it as used
    public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
    {
        return await _db.EmailVerificationTokens
            .FirstOrDefaultAsync(x => x.Token == token);
    }
    // this is used to mark the token as used after successful verification
    public async Task UpdateAsync(EmailVerificationToken token)
    {
        _db.EmailVerificationTokens.Update(token);
        await _db.SaveChangesAsync();
    }
}