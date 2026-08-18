using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services.Auth;

namespace Ube.Infrastructure.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? user?.FindFirst("sub");

            if (userIdClaim == null)
                throw new ForbiddenException("User not authenticated");

            return Guid.Parse(userIdClaim.Value);
        }
    }
    public string Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var emailClaim = user?.FindFirst(ClaimTypes.Email)
                              ?? user?.FindFirst(JwtRegisteredClaimNames.Email);

            if(emailClaim == null)
                throw new ForbiddenException("User Email not found in token");
            return emailClaim.Value;
        }
    }
    public string Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var roleClaim = user?.FindFirst(ClaimTypes.Role);

            if(roleClaim == null)
                throw new ForbiddenException("User Role not found in token");
            return roleClaim.Value;
        }
    }
}