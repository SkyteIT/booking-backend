using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VendorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentVendor()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("User ID not found in token.");
        }

        var vendor = await _context.VendorProfiles
            .Where(v => v.UserId == userId)
            .Select(v => new { v.Id, v.BusinessName })
            .FirstOrDefaultAsync();

        if (vendor == null)
        {
            return NotFound("No vendor profile found for the current user.");
        }

        return Ok(vendor);
    }
}
