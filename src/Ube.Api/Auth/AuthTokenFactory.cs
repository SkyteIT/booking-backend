using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ube.Api.Auth;

public static class AuthTokenFactory
{
    public static TokenValidationParameters ValidationParameters(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"] ?? "UbeLocalDevelopmentKey-ChangeBeforeProduction-32Chars";
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    }

    public static string Create(Guid userId, string email, string role, IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"] ?? "UbeLocalDevelopmentKey-ChangeBeforeProduction-32Chars";
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: new[]
            {
                new System.Security.Claims.Claim("sub", userId.ToString()),
                new System.Security.Claims.Claim("email", email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role),
            },
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}