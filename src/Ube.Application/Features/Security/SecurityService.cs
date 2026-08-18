using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Application.Features.Security;

public class SecurityService : ISecurityService
{
    private readonly IUserRepository _userRepository;

    public SecurityService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new BusinessRuleException("New password and confirmation do not match");

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessRuleException("Account uses social login — password cannot be changed here");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BusinessRuleException("Current password is incorrect");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            throw new BusinessRuleException("New password must be different from the current password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}