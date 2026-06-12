
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

namespace Ube.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;
    private readonly IEmailVerificationRepository _emailVerificationRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService,
        IEmailVerificationRepository emailVerificationRepo,
        IEmailService emailService,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork
    )
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _emailVerificationRepo = emailVerificationRepo;
        _emailService = emailService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    // REGISTER
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException(new[] { "Email is required" });
        var email = request.Email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException(new[] { "Email is required" });

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException(new[] { "Password is required" });

        var exists = await _userRepo.ExistsByEmailAsync(email);

        if (exists)
            throw new BusinessRuleException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? "User" : request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? "User" : request.LastName,
            Role = UserRole.User,
            AuthProvider = AuthProvider.Local,
            GoogleId = null,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        var emailToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };
        await _emailVerificationRepo.AddAsync(emailToken);

        try
        {
            // Email delivery should not block registration if SMTP is misconfigured.
            await _emailService.SendVerificationEmailAsync(user.Email, emailToken.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email for user {UserId} ({Email}).", user.Id, user.Email);
        }

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            VerificationToken = emailToken.Token
        };
    }

    // LOGIN
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException(new[] { "Email and password are required" });
        var email = request.Email.Trim().ToLower();

        var user = await _userRepo.GetByEmailAsync(email);

        if (user == null)
            throw new BusinessRuleException("Invalid credentials");

        if (user.PasswordHash == null)
            throw new BusinessRuleException("Invalid login method");

        var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            throw new BusinessRuleException("Invalid credentials");

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    // GOOGLE LOGIN

    public async Task<AuthResponseDto> GoogleLoginAsync(string token)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(token);

        var email = payload.Email.Trim().ToLower();

        var user = await _userRepo.GetByEmailAsync(email);

        if (user != null && user.AuthProvider == AuthProvider.Local)
        {
            throw new BusinessRuleException("Use email/password login");
        }

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
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
        }
        else
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
        }

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException(new[] { "Token is required" });

        var record = await _emailVerificationRepo.GetByTokenAsync(token);

        if (record == null)
            throw new NotFoundException("Invalid token");

        if (record.IsUsed)
            throw new BusinessRuleException("Token already used");

        if (record.ExpiryDate < DateTime.UtcNow)
            throw new BusinessRuleException("Token expired");

        var user = await _userRepo.GetByIdAsync(record.UserId);

        if (user == null)
            throw new NotFoundException("User not found");
        await _unitOfWork.BeginTransactionAsync();
        try
        {
        // VERIFY USER
            user.IsEmailVerified = true;

            //MARK TOKEN USED
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

    // REFRESH TOKEN
    public async Task<AuthResponseDto> RefreshTokenAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}   