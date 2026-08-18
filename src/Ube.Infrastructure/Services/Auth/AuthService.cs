using System.Security.Cryptography;
using Ube.Domain.Enums.Users;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Common.Interfaces.Persistence;
using Google.Apis.Auth;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Auth;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Auth;
using Ube.Application.Features.Notifications.Email;
using Microsoft.Extensions.Logging;
using Ube.Application.Interfaces;
using Ube.Application.DTOs.Notification;
using Ube.Domain.Enums.Notifications;
using Ube.Application.Common.Interfaces.Services;

namespace Ube.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;
    private readonly IEmailVerificationRepository _emailVerificationRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IRealtimeUpdateService _realtimeUpdateService;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService,
        IEmailVerificationRepository emailVerificationRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IEmailService emailService,
        INotificationService notificationService,
        IRealtimeUpdateService realtimeUpdateService,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _emailVerificationRepo = emailVerificationRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _emailService = emailService;
        _notificationService = notificationService;
        _realtimeUpdateService = realtimeUpdateService;
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

        return await BuildAuthResponseAsync(user);
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
            await NotifyAdminsNewCustomerAsync(user);
        }

        return await BuildAuthResponseAsync(user);
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

        await TryNotifyUserAsync(
            user.Id,
            NotificationType.CustomerAccountVerification,
            "Account verified",
            "Your account email has been verified.");
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
