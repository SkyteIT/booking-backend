using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task <bool> ExistsByEmailAsync(string email);
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
