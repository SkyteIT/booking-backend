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
using Ube.Application.Features.Users;
using Ube.Domain.Enums;

namespace Ube.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(
            IApplicationDbContext context,
            IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // =========================================================
        // REGISTER
        // =========================================================
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            Console.WriteLine("➡ REGISTER STARTED");

            // -------------------------
            // VALIDATION
            // -------------------------
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            var email = request.Email.Trim().ToLower();

            // -------------------------
            // CHECK EXISTING USER
            // -------------------------
            var userExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email);

            if (userExists)
            {
                Console.WriteLine("❌ EMAIL EXISTS");
                throw new Exception("Email already exists");
            }

            // -------------------------
            // DEFAULT VALUES
            // -------------------------
            var firstName = string.IsNullOrWhiteSpace(request.FirstName)
                ? "User"
                : request.FirstName.Trim();

            var lastName = string.IsNullOrWhiteSpace(request.LastName)
                ? "User"
                : request.LastName.Trim();

            // -------------------------
            // CREATE USER
            // -------------------------
            var user = new User
            {
                Id = Guid.NewGuid(),

                Email = email,

                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    request.Password
                ),

                FirstName = firstName,

                LastName = lastName,

                // Customer = 0
                Role = UserRole.Customer,

                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            try
            {
                // -------------------------
                // SAVE USER
                // -------------------------
                var result = await _context.SaveChangesAsync(
                    CancellationToken.None
                );

                if (result <= 0)
                {
                    throw new Exception("Failed to save user");
                }

                Console.WriteLine("✅ USER SAVED");

                // -------------------------
                // RETURN AUTH RESPONSE
                // -------------------------
                return new AuthResponse
                {
                    Token = GenerateToken(user),

                    UserName = $"{user.FirstName} {user.LastName}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "❌ DB ERROR: " + ex.ToString()
                );

                // Keep the original database exception
                // so you can see the actual problem.
                throw;
            }
        }

        // =========================================================
        // LOGIN
        // =========================================================
        public async Task<AuthResponse> LoginAsync(
            LoginRequest request)
        {
            var email = request.Email?
                .Trim()
                .ToLower();

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required");

            // -------------------------
            // FIND USER
            // -------------------------
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email.ToLower() == email
                );

            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            // -------------------------
            // VERIFY PASSWORD
            // -------------------------
            var isValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!isValid)
            {
                throw new Exception("Invalid credentials");
            }

            // -------------------------
            // RETURN RESPONSE
            // -------------------------
            return new AuthResponse
            {
                Token = GenerateToken(user),

                UserName =
                    $"{user.FirstName} {user.LastName}"
            };
        }

        // =========================================================
        // GENERATE JWT TOKEN
        // =========================================================
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                // User ID
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                // Email
                new Claim(
                    ClaimTypes.Email,
                    user.Email
                ),

                // Full name
                new Claim(
                    ClaimTypes.Name,
                    $"{user.FirstName} {user.LastName}"
                ),

                // User role
                // Enum -> String
                new Claim(
                    ClaimTypes.Role,
                    user.Role.ToString()
                )
            };

            // -------------------------
            // JWT KEY
            // -------------------------
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            // -------------------------
            // CREATE TOKEN
            // -------------------------
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],

                audience: _config["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(
                        _config["Jwt:DurationInMinutes"]!
                    )
                ),

                signingCredentials: credentials
            );

            // -------------------------
            // SERIALIZE TOKEN
            // -------------------------
            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // =========================================================
        // GOOGLE LOGIN
        // =========================================================
        public async Task<AuthResponse> GoogleLoginAsync(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception(
                    "Google token is required"
                );
            }

            // -------------------------
            // VALIDATE GOOGLE TOKEN
            // -------------------------
            var payload =
                await GoogleJsonWebSignature.ValidateAsync(token);

            var email = payload.Email
                .Trim()
                .ToLower();

            // -------------------------
            // FIND EXISTING USER
            // -------------------------
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email.ToLower() == email
                );

            // -------------------------
            // CREATE NEW GOOGLE USER
            // -------------------------
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),

                    Email = email,

                    FirstName =
                        payload.GivenName ?? "Google",

                    LastName =
                        payload.FamilyName ?? "User",

                    // Google users are Customers
                    Role = UserRole.Customer,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync(
                    CancellationToken.None
                );
            }

            // -------------------------
            // RETURN RESPONSE
            // -------------------------
            return new AuthResponse
            {
                Token = GenerateToken(user),

                UserName =
                    $"{user.FirstName} {user.LastName}"
            };
        }
    }
}