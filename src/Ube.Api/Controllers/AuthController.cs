using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces;
using Ube.Domain.Entities.Users;
using Ube.Application.Common.Interfaces.Services.Auth;

namespace Ube.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet("test-token")]
    public IActionResult GetTestToken()
    {
        var user = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "test@test.com"
        };

        var token = _tokenService.GenerateToken(user);

        return Ok(token);
    }
}