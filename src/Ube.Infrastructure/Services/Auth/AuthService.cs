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
using Ube.Application.Features.Notifications;
using Ube.Application.Features.Notifications.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using Ube.Domain.Enums.Notifications;

namespace Ube.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;
    private readonly IEmailVerificationRepository _emailVerificationRepo;
    private readonly IPasswordResetRepository _passwordResetRepo;
    private readonly ITwoFactorChallengeRepository _twoFactorRepo;
    private readonly ITwoFactorBackupCodeRepository _backupCodeRepo;
    private readonly ITrustedDeviceRepository _trustedDeviceRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IEmailService _emailService;
    private readonly IEncryptionService _encryptionService;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeUpdateService _realtimeUpdateService;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _googleClientId;

    private const int BackupCodeCount = 10;
    private const string TotpIssuer = "Ube";
    private const int TrustedDeviceDays = 7;

    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService,
        IEmailVerificationRepository emailVerificationRepo,
        IPasswordResetRepository passwordResetRepo,
        ITwoFactorChallengeRepository twoFactorRepo,
        ITwoFactorBackupCodeRepository backupCodeRepo,
        ITrustedDeviceRepository trustedDeviceRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IEmailService emailService,
        IEncryptionService encryptionService,
        INotificationService notificationService,
        IRealtimeUpdateService realtimeUpdateService,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _emailVerificationRepo = emailVerificationRepo;
        _passwordResetRepo = passwordResetRepo;
        _twoFactorRepo = twoFactorRepo;
        _backupCodeRepo = backupCodeRepo;
        _trustedDeviceRepo = trustedDeviceRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _emailService = emailService;
        _encryptionService = encryptionService;
        _notificationService = notificationService;
        _realtimeUpdateService = realtimeUpdateService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _googleClientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is not configured.");
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

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send welcome email for user {UserId} ({Email}).", user.Id, user.Email);
        }

        await NotifyAdminsNewCustomerAsync(user);

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

        return await CompleteLoginOrChallengeAsync(user, request.DeviceToken);
    }
    public async Task<AuthResponseDto> GoogleLoginAsync(string idToken, string? deviceToken = null)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            });
        }
        catch (Exception ex) when (ex is InvalidJwtException or FormatException)
        {
            throw new BusinessRuleException("Invalid or expired Google sign-in token.");
        }
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
            await NotifyAdminsNewCustomerAsync(user);
        }

        return await CompleteLoginOrChallengeAsync(user, deviceToken);
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
            if (record.PendingEmail != null)
            {
                // Email-change confirmation, not a registration verify -
                // deliberately does NOT touch IsEmailVerified, which
                // doubles as this account's suspension flag (see
                // AdminService.UpdateUserStatusAsync). Flipping it here
                // would let a suspended account un-suspend itself just by
                // changing its email.
                if (await _userRepo.ExistsByEmailAsync(record.PendingEmail))
                    throw new BusinessRuleException("This email is now in use by another account");

                user.Email = record.PendingEmail;
            }
            else
            {
                user.IsEmailVerified = true;
            }

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

        await TryNotifyUserAsync(
            user.Id,
            NotificationType.CustomerAccountVerification,
            "Account verified",
            "Your account email has been verified.");
    }

    public async Task RequestEmailChangeAsync(Guid userId, string newEmail)
    {
        var normalized = newEmail.Trim().ToLower();

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (normalized == user.Email)
            throw new BusinessRuleException("This is already your email address");

        if (await _userRepo.ExistsByEmailAsync(normalized))
            throw new BusinessRuleException("This email is already in use");

        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            PendingEmail = normalized
        };
        await _emailVerificationRepo.AddAsync(token);

        await _emailService.SendEmailChangeVerificationEmailAsync(normalized, token.Token);
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
    //
    // Exception: a deviceToken matching an unexpired TrustedDevice (from an
    // earlier "remember this device" verification) skips the challenge
    // entirely - same pattern WhatsApp Web uses so a recognized browser
    // isn't re-prompted on every single login.
    private async Task<AuthResponseDto> CompleteLoginOrChallengeAsync(User user, string? deviceToken = null)
    {
        if (user.Role is UserRole.Admin or UserRole.Finance or UserRole.SuperAdmin)
        {
            if (!string.IsNullOrEmpty(deviceToken))
            {
                var trusted = await _trustedDeviceRepo.GetValidAsync(user.Id, HashDeviceToken(deviceToken));
                if (trusted != null)
                {
                    // Sliding window, like WhatsApp - using the device
                    // resets its 7-day trust period instead of it quietly
                    // expiring on a regularly-used browser.
                    trusted.ExpiresAt = DateTime.UtcNow.AddDays(TrustedDeviceDays);
                    await _trustedDeviceRepo.UpdateAsync(trusted);
                    return await BuildAuthResponseAsync(user);
                }
            }

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

    public async Task<TwoFactorEnrollmentResultDto> ConfirmTwoFactorEnrollmentAsync(string challengeToken, string code, bool rememberDevice = false)
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

            if (rememberDevice)
            {
                var rawToken = GenerateSecureToken();
                await _trustedDeviceRepo.AddAsync(new TrustedDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = HashDeviceToken(rawToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(TrustedDeviceDays)
                });
                auth.DeviceToken = rawToken;
            }

            return new TwoFactorEnrollmentResultDto { Auth = auth, BackupCodes = backupCodes };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponseDto> VerifyTwoFactorCodeAsync(string challengeToken, string code, bool rememberDevice = false)
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

        var auth = await BuildAuthResponseAsync(user);

        if (rememberDevice)
        {
            var rawToken = GenerateSecureToken();
            await _trustedDeviceRepo.AddAsync(new TrustedDevice
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashDeviceToken(rawToken),
                ExpiresAt = DateTime.UtcNow.AddDays(TrustedDeviceDays)
            });
            auth.DeviceToken = rawToken;
        }

        return auth;
    }

    public async Task<TwoFactorEnrollmentStartDto> StartSelfServiceTwoFactorEnrollmentAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

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

    public async Task<List<string>> ConfirmSelfServiceTwoFactorEnrollmentAsync(Guid userId, string code)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

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
            await _userRepo.UpdateAsync(user);

            var backupCodes = GenerateBackupCodes(user.Id, out var hashedCodes);
            await _backupCodeRepo.AddRangeAsync(hashedCodes);

            await _unitOfWork.CommitAsync();
            return backupCodes;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DisableTwoFactorAsync(Guid userId, string currentPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!user.TwoFactorEnabled)
            throw new BusinessRuleException("Two-factor authentication is not enabled");

        // Admin/Finance/SuperAdmin are forced through the 2FA challenge on every
        // login regardless of this flag (see CompleteLoginOrChallengeAsync) - if
        // disable were allowed here, TwoFactorSecret gets nulled below and the
        // very next login would force a brand-new QR enrollment, every time,
        // forever. Disabling was never actually a supported end-state for these
        // roles; reject it outright instead of leaving that trap in place.
        if (user.Role is UserRole.Admin or UserRole.Finance or UserRole.SuperAdmin)
            throw new BusinessRuleException("Two-factor authentication is required for your role and cannot be disabled.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessRuleException("Account uses social login — password cannot be verified here");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            throw new BusinessRuleException("Current password is incorrect");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _userRepo.UpdateAsync(user);

            // Unused backup codes for a disabled 2FA setup are dead weight and
            // a latent risk if 2FA is ever re-enabled and the old codes leak.
            await _backupCodeRepo.DeleteAllForUserAsync(user.Id);

            // Trusted devices only exist to skip a 2FA challenge - with 2FA
            // off there's nothing left for them to skip, and keeping them
            // around would silently un-expire if 2FA is ever re-enabled.
            await _trustedDeviceRepo.DeleteAllForUserAsync(user.Id);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
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
            ProfileImageUrl = user.ProfileImageUrl,
            TwoFactorEnabled = user.TwoFactorEnabled,
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task<CurrentUserDto> UpdateProfileImageAsync(Guid userId, string imageUrl)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        user.ProfileImageUrl = imageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);

        return new CurrentUserDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ProfileImageUrl = user.ProfileImageUrl,
            TwoFactorEnabled = user.TwoFactorEnabled,
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);

        return new CurrentUserDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            ProfileImageUrl = user.ProfileImageUrl,
            TwoFactorEnabled = user.TwoFactorEnabled,
            PhoneNumber = user.PhoneNumber
        };
    }

    // --- helpers ---

    private static string GenerateSecureToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

    // Only the hash is ever persisted - the raw token lives in the client's
    // browser storage and is the actual bearer credential, so it must never
    // be recoverable from the database.
    private static string HashDeviceToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));

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

    private async Task NotifyAdminsNewCustomerAsync(User user)
    {
        var admins = await _userRepo.GetByRoleAsync(UserRole.Admin);
        foreach (var admin in admins)
        {
            try
            {
                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserId = admin.Id,
                    Title = "New customer registration",
                    Message = $"New customer registered: {user.Email}",
                    Type = (int)NotificationType.AdminNewCustomerRegistration
                }, CancellationToken.None);
            }
            catch
            {
                // best-effort only
            }
        }

        try
        {
            await _realtimeUpdateService.PublishToRoleAsync(
                "admin",
                "dashboard.refresh",
                new
                {
                    reason = "customer.registered",
                    userId = user.Id,
                    email = user.Email
                });
        }
        catch
        {
            // Realtime refresh is best-effort.
        }
    }

    private async Task TryNotifyUserAsync(Guid userId, NotificationType type, string title, string message)
    {
        try
        {
            await _notificationService.CreateAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = (int)type
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create notification {NotificationType} for user {UserId}", type, userId);
        }
    }
}
