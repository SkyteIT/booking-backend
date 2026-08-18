using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Ube.Application.Common.Models.JWT;
using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;
using Ube.Infrastructure.Services.Auth;

namespace Ube.Tests.Auth;

public class TokenServiceTests
{
    private static TokenService Build()
    {
        var settings = new JwtSettings
        {
            Key = "this-is-a-test-signing-key-32-bytes-min",
            Issuer = "UbeApp",
            Audience = "UbeAppUsers",
            ExpiryMinutes = 60
        };
        return new TokenService(Options.Create(settings));
    }

    private static User MakeUser(UserRole role) => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@ube.local",
        FirstName = "Test",
        LastName = "User",
        Role = role
    };

    private static List<string> RoleClaims(string jwt)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        return token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    [Fact]
    public void GenerateToken_SuperAdmin_Carries_All_Three_Role_Claims()
    {
        var service = Build();
        var (jwt, _) = service.GenerateToken(MakeUser(UserRole.SuperAdmin));

        var roles = RoleClaims(jwt);

        Assert.Equal(3, roles.Count);
        Assert.Equal("SuperAdmin", roles[0]); // first, so FindFirst reads the real role
        Assert.Contains("Admin", roles);
        Assert.Contains("Finance", roles);
    }

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Vendor)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Finance)]
    public void GenerateToken_NonSuperAdmin_Carries_Exactly_One_Role_Claim(UserRole role)
    {
        var service = Build();
        var (jwt, _) = service.GenerateToken(MakeUser(role));

        var roles = RoleClaims(jwt);

        Assert.Single(roles);
        Assert.Equal(role.ToString(), roles[0]);
    }
}
