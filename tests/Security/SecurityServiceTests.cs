using Moq;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Features.Security;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Tests.Security;

public class SecurityServiceTests
{
    private static SecurityService BuildService(Mock<IUserRepository> userRepo)
        => new SecurityService(userRepo.Object);

    private static User MakeLocalUser(string passwordHash) => new User
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        FirstName = "Test",
        LastName = "User",
        Role = UserRole.User,
        AuthProvider = AuthProvider.Local,
        PasswordHash = passwordHash
    };

    private static ChangePasswordDto MakeDto(string current, string newPwd, string confirm) =>
        new ChangePasswordDto
        {
            CurrentPassword = current,
            NewPassword = newPwd,
            ConfirmPassword = confirm
        };

    // --- ConfirmPassword mismatch ---

    [Fact]
    public async Task ChangePassword_Throws_When_ConfirmPassword_Does_Not_Match()
    {
        var repo = new Mock<IUserRepository>();
        var service = BuildService(repo);

        var dto = MakeDto("OldPass1!", "NewPass1!", "DifferentPass1!");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ChangePasswordAsync(Guid.NewGuid(), dto));

        Assert.Contains("do not match", ex.Message, StringComparison.OrdinalIgnoreCase);
        repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    // --- User not found ---

    [Fact]
    public async Task ChangePassword_Throws_NotFoundException_When_User_Does_Not_Exist()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var service = BuildService(repo);
        var dto = MakeDto("OldPass1!", "NewPass1!", "NewPass1!");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ChangePasswordAsync(Guid.NewGuid(), dto));
    }

    // --- Social login user (no password hash) ---

    [Fact]
    public async Task ChangePassword_Throws_When_User_Has_No_Password_Hash()
    {
        var userId = Guid.NewGuid();
        var user = MakeLocalUser(string.Empty);
        user.PasswordHash = null;

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = BuildService(repo);
        var dto = MakeDto("anything", "NewPass1!", "NewPass1!");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ChangePasswordAsync(userId, dto));

        Assert.Contains("social login", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- Wrong current password ---

    [Fact]
    public async Task ChangePassword_Throws_When_Current_Password_Is_Wrong()
    {
        var userId = Guid.NewGuid();
        var hash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1!");
        var user = MakeLocalUser(hash);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = BuildService(repo);
        var dto = MakeDto("WrongPass1!", "NewPass1!", "NewPass1!");

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ChangePasswordAsync(userId, dto));

        Assert.Contains("incorrect", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- New password same as current ---

    [Fact]
    public async Task ChangePassword_Throws_When_New_Password_Same_As_Current()
    {
        var userId = Guid.NewGuid();
        const string password = "SamePass1!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = MakeLocalUser(hash);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = BuildService(repo);
        var dto = MakeDto(password, password, password);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ChangePasswordAsync(userId, dto));

        Assert.Contains("different", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- Happy path ---

    [Fact]
    public async Task ChangePassword_Succeeds_And_Updates_Hash()
    {
        var userId = Guid.NewGuid();
        const string oldPassword = "OldPass1!";
        const string newPassword = "NewPass1!";
        var hash = BCrypt.Net.BCrypt.HashPassword(oldPassword);
        var user = MakeLocalUser(hash);

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = BuildService(repo);
        var dto = MakeDto(oldPassword, newPassword, newPassword);

        await service.ChangePasswordAsync(userId, dto);

        repo.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            BCrypt.Net.BCrypt.Verify(newPassword, u.PasswordHash!) &&
            u.UpdatedAt != null
        )), Times.Once);
    }
}
