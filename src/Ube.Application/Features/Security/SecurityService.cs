using Microsoft.AspNetCore.Identity;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Domain.Entities.Users;

public class SecurityService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public SecurityService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
{
    try
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessRuleException("Password not set");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.CurrentPassword
        );

        if (result == PasswordVerificationResult.Failed)
            throw new BusinessRuleException("Current password is incorrect");

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
    catch (Exception ex)
    {
        // DEBUG HERE
        Console.WriteLine("ERROR: " + ex.Message);
        Console.WriteLine(ex.StackTrace);

        throw; // keep middleware handling
    }
}
}