using Ube.Domain.Entities.Auth;

namespace Ube.Application.Common.Interfaces.Persistence;
    
public interface IEmailVerificationRepository
{
    Task AddAsync(EmailVerificationToken token);
    Task<EmailVerificationToken?> GetByTokenAsync(string token);
    Task UpdateAsync(EmailVerificationToken token);
}