using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Google.Apis.Auth;
using Ube.Domain.Entities.Users;
using Ube.Application.Common.Interfaces;
using Ube.Application.Interfaces;
using Ube.Application.Features.Users;

namespace Ube.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(IApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // =========================
        // REGISTER
        // =========================
        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            Console.WriteLine("➡ REGISTER STARTED");

            // VALIDATION
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            var email = request.Email.Trim().ToLower();

            // CHECK EXISTING USER (SAFE)
            var userExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email);

            if (userExists)
            {
                Console.WriteLine("❌ EMAIL EXISTS");
                throw new Exception("Email already exists");
            }

            // DEFAULT VALUES (DON'T MODIFY REQUEST OBJECT)
            var firstName = string.IsNullOrWhiteSpace(request.FirstName)
                ? "User"
                : request.FirstName;

            var lastName = string.IsNullOrWhiteSpace(request.LastName)
                ? "User"
                : request.LastName;

            // CREATE USER
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = firstName,
                LastName = lastName,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            try
            {
                var result = await _context.SaveChangesAsync(CancellationToken.None);

                if (result <= 0)
                    throw new Exception("Failed to save user");

                Console.WriteLine("✅ USER SAVED");

                return GenerateToken(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ DB ERROR: " + ex.Message);
                throw new Exception("Database error while creating user");
            }
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<string> LoginAsync(LoginRequest request)
        {
            var email = request.Email?.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
                throw new Exception("Invalid credentials");

            var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValid)
                throw new Exception("Invalid credentials");

            return GenerateToken(user);
        }

        // =========================
        // JWT TOKEN
        // =========================
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_config["Jwt:DurationInMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GoogleLoginAsync(string token)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token);

            var email = payload.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = payload.GivenName ?? "Google",
                    LastName = payload.FamilyName ?? "User",
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            return GenerateToken(user);
        }
    }
}