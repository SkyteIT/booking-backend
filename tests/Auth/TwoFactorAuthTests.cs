using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Auth;
using Ube.Application.Features.Notifications.Email;
using Ube.Domain.Entities.Auth;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;
using Ube.Infrastructure.Services.Auth;

namespace Ube.Tests.Auth;

public class TwoFactorAuthTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IEmailVerificationRepository> _emailVerificationRepo = new();
    private readonly Mock<IPasswordResetRepository> _passwordResetRepo = new();
    private readonly Mock<ITwoFactorChallengeRepository> _twoFactorRepo = new();
    private readonly Mock<ITwoFactorBackupCodeRepository> _backupCodeRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEncryptionService> _encryptionService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AuthService BuildService()
    {
        // Pass-through fake - not re-testing real encryption here, just the
        // 2FA glue logic that calls it.
        _encryptionService.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
        _encryptionService.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);

        _tokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns(("fake-jwt", DateTime.UtcNow.AddHours(1)));

        return new AuthService(
            _userRepo.Object,
            _tokenService.Object,
            _emailVerificationRepo.Object,
            _passwordResetRepo.Object,
            _twoFactorRepo.Object,
            _backupCodeRepo.Object,
            _refreshTokenRepo.Object,
            _emailService.Object,
            _encryptionService.Object,
            Mock.Of<ILogger<AuthService>>(),
            _unitOfWork.Object);
    }

    private static User MakeUser(UserRole role, bool twoFactorEnabled = false, string? secret = null) => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User",
        Role = role,
        AuthProvider = AuthProvider.Local,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
        TwoFactorEnabled = twoFactorEnabled,
        TwoFactorSecret = secret
    };

    private static string CurrentTotpCode(string base32Secret)
        => new Totp(Base32Encoding.ToBytes(base32Secret)).ComputeTotp();

    // --- LoginAsync: role-based 2FA gating ---

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Vendor)]
    public async Task LoginAsync_Issues_Real_Tokens_Immediately_For_NonPrivileged_Roles(UserRole role)
    {
        var user = MakeUser(role);
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var service = BuildService();

        var result = await service.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "Password1!" });

        Assert.False(result.RequiresTwoFactor);
        Assert.Equal("fake-jwt", result.Token);
        _twoFactorRepo.Verify(r => r.AddAsync(It.IsAny<TwoFactorChallenge>()), Times.Never);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Finance)]
    public async Task LoginAsync_Returns_Challenge_Not_Tokens_For_Privileged_Roles(UserRole role)
    {
        var user = MakeUser(role, twoFactorEnabled: false);
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var service = BuildService();

        var result = await service.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "Password1!" });

        Assert.True(result.RequiresTwoFactor);
        Assert.True(result.RequiresEnrollment);
        Assert.Equal(string.Empty, result.Token);
        Assert.NotNull(result.ChallengeToken);
        _twoFactorRepo.Verify(r => r.AddAsync(It.IsAny<TwoFactorChallenge>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_RequiresEnrollment_Is_False_When_Already_Enrolled()
    {
        var user = MakeUser(UserRole.Admin, twoFactorEnabled: true, secret: "SECRET123");
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        var service = BuildService();

        var result = await service.LoginAsync(new LoginRequestDto { Email = user.Email, Password = "Password1!" });

        Assert.True(result.RequiresTwoFactor);
        Assert.False(result.RequiresEnrollment);
    }

    // --- StartTwoFactorEnrollmentAsync ---

    [Fact]
    public async Task StartEnrollment_Throws_NotFound_For_Invalid_Challenge()
    {
        _twoFactorRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((TwoFactorChallenge?)null);
        var service = BuildService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.StartTwoFactorEnrollmentAsync("bad-token"));
    }

    [Fact]
    public async Task StartEnrollment_Throws_When_Challenge_Expired()
    {
        var user = MakeUser(UserRole.Admin);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(-1) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.StartTwoFactorEnrollmentAsync("t"));
    }

    [Fact]
    public async Task StartEnrollment_Throws_When_Already_Enabled()
    {
        var user = MakeUser(UserRole.Admin, twoFactorEnabled: true, secret: "EXISTING");
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.StartTwoFactorEnrollmentAsync("t"));
    }

    [Fact]
    public async Task StartEnrollment_Succeeds_And_Stores_Secret()
    {
        var user = MakeUser(UserRole.Admin);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        var result = await service.StartTwoFactorEnrollmentAsync("t");

        Assert.False(string.IsNullOrEmpty(result.Secret));
        Assert.Contains("otpauth://totp/", result.OtpAuthUri);
        _userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.TwoFactorSecret == result.Secret)), Times.Once);
    }

    // --- ConfirmTwoFactorEnrollmentAsync ---

    [Fact]
    public async Task ConfirmEnrollment_Throws_For_Invalid_Code()
    {
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var user = MakeUser(UserRole.Admin, secret: secret);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.ConfirmTwoFactorEnrollmentAsync("t", "000000"));
    }

    [Fact]
    public async Task ConfirmEnrollment_Succeeds_With_Valid_Code_And_Returns_BackupCodes()
    {
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var user = MakeUser(UserRole.Admin, secret: secret);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        var code = CurrentTotpCode(secret);
        var result = await service.ConfirmTwoFactorEnrollmentAsync("t", code);

        Assert.Equal(10, result.BackupCodes.Count);
        Assert.Equal(10, result.BackupCodes.Distinct().Count());
        Assert.Equal("fake-jwt", result.Auth.Token);
        _userRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.TwoFactorEnabled)), Times.Once);
        _backupCodeRepo.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<TwoFactorBackupCode>>(c => c.Count() == 10)), Times.Once);
    }

    // --- VerifyTwoFactorCodeAsync ---

    [Fact]
    public async Task Verify_Succeeds_With_Valid_Totp_Code()
    {
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var user = MakeUser(UserRole.Admin, twoFactorEnabled: true, secret: secret);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var service = BuildService();

        var result = await service.VerifyTwoFactorCodeAsync("t", CurrentTotpCode(secret));

        Assert.Equal("fake-jwt", result.Token);
        Assert.True(challenge.IsUsed);
    }

    [Fact]
    public async Task Verify_Falls_Back_To_Valid_Unused_BackupCode()
    {
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var user = MakeUser(UserRole.Admin, twoFactorEnabled: true, secret: secret);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        const string plainBackupCode = "ABCDEF1234";
        var backupCode = new TwoFactorBackupCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = BCrypt.Net.BCrypt.HashPassword(plainBackupCode),
            IsUsed = false
        };
        _backupCodeRepo.Setup(r => r.GetUnusedByUserIdAsync(user.Id)).ReturnsAsync(new List<TwoFactorBackupCode> { backupCode });

        var service = BuildService();

        var result = await service.VerifyTwoFactorCodeAsync("t", plainBackupCode);

        Assert.Equal("fake-jwt", result.Token);
        _backupCodeRepo.Verify(r => r.UpdateAsync(It.Is<TwoFactorBackupCode>(c => c.IsUsed)), Times.Once);
    }

    [Fact]
    public async Task Verify_Throws_When_Code_Is_Neither_Valid_Totp_Nor_Backup_Code()
    {
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var user = MakeUser(UserRole.Admin, twoFactorEnabled: true, secret: secret);
        var challenge = new TwoFactorChallenge { Id = Guid.NewGuid(), UserId = user.Id, Token = "t", ExpiryDate = DateTime.UtcNow.AddMinutes(5) };
        _twoFactorRepo.Setup(r => r.GetByTokenAsync("t")).ReturnsAsync(challenge);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _backupCodeRepo.Setup(r => r.GetUnusedByUserIdAsync(user.Id)).ReturnsAsync(new List<TwoFactorBackupCode>());

        var service = BuildService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.VerifyTwoFactorCodeAsync("t", "000000"));
    }
}
