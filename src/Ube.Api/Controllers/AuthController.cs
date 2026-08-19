using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ube.Api.Auth;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(item => item.Email == request.Email);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var role = user.Role.ToString();
        return Ok(new
        {
            token = AuthTokenFactory.Create(user.Id, user.Email, role, _configuration),
            user = new { id = user.Id, email = user.Email, firstName = user.FirstName, lastName = user.LastName, role },
            role,
        });
    }

    [Authorize]
    [HttpGet("current-user")]
    public async Task<IActionResult> CurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        if (user is null) return Unauthorized();

        return Ok(new
        {
            id = user.Id,
            userId = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            role = user.Role.ToString(),
        });
    }
}

public sealed record LoginRequest(string Email, string Password);