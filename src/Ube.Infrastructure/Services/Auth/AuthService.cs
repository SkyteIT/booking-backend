using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ube.Domain.Enums.Users;
using BCrypt.Net;
using Google.Apis.Auth;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Auth;
using Ube.Domain.Entities.Users;
using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;

    public AuthService(
        IUserRepository userRepo,
        ITokenService tokenService)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
    }

    // =========================
    // REGISTER
    // =========================
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
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
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? "User" : request.FirstName,
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? "User" : request.LastName,
            Role = UserRole.User,
            AuthProvider = AuthProvider.Google,
            GoogleId = null,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        return new AuthResponse
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    // =========================
    // LOGIN
    // =========================
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
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

        return new AuthResponse
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    // =========================
    // GOOGLE LOGIN
    // =========================
    public async Task<AuthResponse> GoogleLoginAsync(string token)
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

        return new AuthResponse
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}   