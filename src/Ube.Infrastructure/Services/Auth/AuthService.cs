using System.Security.Cryptography;
using Ube.Domain.Enums.Users;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Common.Interfaces.Services;
using Ube.Application.Common.Interfaces.Persistence;
using Google.Apis.Auth;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Auth;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Auth;
using Ube.Application.Features.Notifications.Email;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace Ube.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;
    private readonly IEmailVerificationRepository _emailVerificationRepo;
    private readonly IPasswordResetRepository _passwordResetRepo;
    private readonly ITwoFactorChallengeRepository _twoFactorRepo;
    private readonly ITwoFactorBackupCodeRepository _backupCodeRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IEmailService _emailService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    private const int BackupCodeCount = 10;
    private const string TotpIssuer = "Ube";

    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService,
        IEmailVerificationRepository emailVerificationRepo,
        IPasswordResetRepository passwordResetRepo,
        ITwoFactorChallengeRepository twoFactorRepo,
        ITwoFactorBackupCodeRepository backupCodeRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IEmailService emailService,
        IEncryptionService encryptionService,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _emailVerificationRepo = emailVerificationRepo;
        _passwordResetRepo = passwordResetRepo;
        _twoFactorRepo = twoFactorRepo;
        _backupCodeRepo = backupCodeRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _emailService = emailService;
        _encryptionService = encryptionService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var exists = await _userRepo.ExistsByEmailAsync(email);

        if (exists)
            throw new BusinessRuleException("Email already in use");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? "User" : request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? "User" : request.LastName,
            Role = UserRole.User,
            AuthProvider = AuthProvider.Local,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        var emailToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateSecureToken(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };
        await _emailVerificationRepo.AddAsync(emailToken);

        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, emailToken.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email for user {UserId} ({Email}).", user.Id, user.Email);
        }

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _userRepo.GetByEmailAsync(email);

        if (user == null || user.PasswordHash == null)
            throw new BusinessRuleException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BusinessRuleException("Invalid credentials");

        return await CompleteLoginOrChallengeAsync(user);
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        var email = payload.Email.Trim().ToLower();
        var user = await _userRepo.GetByEmailAsync(email);

        if (user != null && user.AuthProvider == AuthProvider.Local)
            throw new BusinessRuleException("This email is registered with email/password. Please use that login method.");

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = payload.GivenName ?? "Google",
                LastName = payload.FamilyName ?? "User",
                Role = UserRole.User,
                AuthProvider = AuthProvider.Google,
                GoogleId = payload.Subject,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepo.AddAsync(user);
        }

        return await CompleteLoginOrChallengeAsync(user);
    }

    public async Task VerifyEmailAsync(string token)
    {
        var record = await _emailVerificationRepo.GetByTokenAsync(token);

        if (record == null)
            throw new NotFoundException("Invalid or expired token");

        if (record.IsUsed)
            throw new BusinessRuleException("Token has already been used");

        if (record.ExpiryDate < DateTime.UtcNow)
            throw new BusinessRuleException("Token has expired");

        var user = await _userRepo.GetByIdAsync(record.UserId)
            ?? throw new NotFoundException("User not found");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.IsEmailVerified = true;
            record.IsUsed = true;
            await _userRepo.UpdateAsync(user);
            await _emailVerificationRepo.UpdateAsync(record);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email.Trim().ToLower());

        // No user enumeration: if the account doesn't exist, or is a
        // Google-auth account with no password to reset, silently do
        // nothing. The controller always returns the same generic response
        // either way, so this never reveals whether the email is registered.
        if (user == null || user.PasswordHash == null)
            return;

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateSecureToken(),
            ExpiryDate = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };
        await _passwordResetRepo.AddAsync(resetToken);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset email for user {UserId} ({Email}).", user.Id, user.Email);
        }
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var record = await _passwordResetRepo.GetByTokenAsync(token);

        if (record == null)
            throw new NotFoundException("Invalid or expired token");

        if (record.IsUsed)
            throw new BusinessRuleException("Token has already been used");

        if (record.ExpiryDate < DateTime.UtcNow)
            throw new BusinessRuleException("Token has expired");

        var user = await _userRepo.GetByIdAsync(record.UserId)
            ?? throw new NotFoundException("User not found");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            record.IsUsed = true;

            await _userRepo.UpdateAsync(user);
            await _passwordResetRepo.UpdateAsync(record);

            // Force every other session to re-authenticate after a password reset.
            await _refreshTokenRepo.RevokeAllForUserAsync(user.Id);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    // Admin/Finance logins never go straight to a token - they get an opaque,
    // short-lived challenge instead. Real tokens only get issued once the
    // challenge is resolved via enrollment or verification below.
    private async Task<AuthResponseDto> CompleteLoginOrChallengeAsync(User user)
    {
        if (user.Role is UserRole.Admin or UserRole.Finance)
        {
            var challenge = new TwoFactorChallenge
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = GenerateSecureToken(),
                ExpiryDate = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };
            await _twoFactorRepo.AddAsync(challenge);

            return new AuthResponseDto
            {
                RequiresTwoFactor = true,
                RequiresEnrollment = !user.TwoFactorEnabled,
                ChallengeToken = challenge.Token
            };
        }

        return await BuildAuthResponseAsync(user);
    }

    private async Task<(User User, TwoFactorChallenge Challenge)> ValidateChallengeAsync(string challengeToken)
    {
        var challenge = await _twoFactorRepo.GetByTokenAsync(challengeToken);

        if (challenge == null)
            throw new NotFoundException("Invalid or expired challenge");

        if (challenge.IsUsed)
            throw new BusinessRuleException("Challenge has already been used");

        if (challenge.ExpiryDate < DateTime.UtcNow)
            throw new BusinessRuleException("Challenge has expired");

        var user = await _userRepo.GetByIdAsync(challenge.UserId)
            ?? throw new NotFoundException("User not found");

        return (user, challenge);
    }

    public async Task<TwoFactorEnrollmentStartDto> StartTwoFactorEnrollmentAsync(string challengeToken)
    {
        var (user, _) = await ValidateChallengeAsync(challengeToken);

        if (user.TwoFactorEnabled)
            throw new BusinessRuleException("Two-factor authentication is already enabled");

        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretBytes);

        user.TwoFactorSecret = _encryptionService.Encrypt(base32Secret);
        await _userRepo.UpdateAsync(user);

        var otpAuthUri = $"otpauth://totp/{TotpIssuer}:{Uri.EscapeDataString(user.Email)}" +
                          $"?secret={base32Secret}&issuer={TotpIssuer}&digits=6&period=30";

        return new TwoFactorEnrollmentStartDto
        {
            Secret = base32Secret,
            OtpAuthUri = otpAuthUri
        };
    }

    public async Task<TwoFactorEnrollmentResultDto> ConfirmTwoFactorEnrollmentAsync(string challengeToken, string code)
    {
        var (user, challenge) = await ValidateChallengeAsync(challengeToken);

        if (user.TwoFactorEnabled)
            throw new BusinessRuleException("Two-factor authentication is already enabled");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new BusinessRuleException("Enrollment has not been started");

        var base32Secret = _encryptionService.Decrypt(user.TwoFactorSecret);
        var totp = new Totp(Base32Encoding.ToBytes(base32Secret));

        if (!totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            throw new BusinessRuleException("Invalid verification code");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.TwoFactorEnabled = true;
            challenge.IsUsed = true;

            await _userRepo.UpdateAsync(user);
            await _twoFactorRepo.UpdateAsync(challenge);

            var backupCodes = GenerateBackupCodes(user.Id, out var hashedCodes);
            await _backupCodeRepo.AddRangeAsync(hashedCodes);

            await _unitOfWork.CommitAsync();

            var auth = await BuildAuthResponseAsync(user);
            return new TwoFactorEnrollmentResultDto { Auth = auth, BackupCodes = backupCodes };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponseDto> VerifyTwoFactorCodeAsync(string challengeToken, string code)
    {
        var (user, challenge) = await ValidateChallengeAsync(challengeToken);

        if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new BusinessRuleException("Two-factor authentication is not enabled for this account");

        var base32Secret = _encryptionService.Decrypt(user.TwoFactorSecret);
        var totp = new Totp(Base32Encoding.ToBytes(base32Secret));

        var codeValid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        if (!codeValid)
        {
            // Not a valid TOTP code - check whether it matches an unused backup code instead.
            var unusedCodes = await _backupCodeRepo.GetUnusedByUserIdAsync(user.Id);
            var matchedBackupCode = unusedCodes.FirstOrDefault(c => BCrypt.Net.BCrypt.Verify(code, c.CodeHash));

            if (matchedBackupCode == null)
                throw new BusinessRuleException("Invalid verification code");

            matchedBackupCode.IsUsed = true;
            matchedBackupCode.UsedAt = DateTime.UtcNow;
            await _backupCodeRepo.UpdateAsync(matchedBackupCode);
        }

        challenge.IsUsed = true;
        await _twoFactorRepo.UpdateAsync(challenge);

        return await BuildAuthResponseAsync(user);
    }

    private static List<string> GenerateBackupCodes(Guid userId, out List<TwoFactorBackupCode> hashedCodes)
    {
        var plainCodes = new List<string>();
        hashedCodes = new List<TwoFactorBackupCode>();

        for (var i = 0; i < BackupCodeCount; i++)
        {
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)); // 10 hex chars
            plainCodes.Add(code);
            hashedCodes.Add(new TwoFactorBackupCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
                IsUsed = false
            });
        }

        return plainCodes;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepo.GetByTokenAsync(refreshToken)
            ?? throw new BusinessRuleException("Invalid refresh token");

        if (stored.IsRevoked)
            throw new BusinessRuleException("Refresh token has been revoked");

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("Refresh token has expired");

        // Rotate: revoke the used token and issue a new one
        stored.IsRevoked = true;
        await _refreshTokenRepo.UpdateAsync(stored);

        return await BuildAuthResponseAsync(stored.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
        if (stored == null || stored.IsRevoked) return;

        stored.IsRevoked = true;
        await _refreshTokenRepo.UpdateAsync(stored);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return null;

        return new CurrentUserDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    // --- helpers ---

    private static string GenerateSecureToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
    {
        var (jwtToken, expiresAt) = _tokenService.GenerateToken(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepo.AddAsync(refreshToken);

        return new AuthResponseDto
        {
            Token = jwtToken,
            TokenExpiresAt = expiresAt,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}