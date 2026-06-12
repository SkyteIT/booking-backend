using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;


public class SecurityService
{
    private readonly IUserRepository _userRepository;

    public SecurityService(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessRuleException("Password not set");
        // compare hash of current password with stored hash
        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
            throw new BusinessRuleException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}