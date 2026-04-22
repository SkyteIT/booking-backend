using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VendorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Temporary endpoint to get a mock logged-in vendor
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentVendor()
    {
        // For development: just get the first vendor. If none exists, return 404.
        var vendor = await _context.VendorProfiles
            .Select(v => new { v.Id, v.BusinessName })
            .FirstOrDefaultAsync();

        if (vendor == null)
        {
            return NotFound("No vendors found in the database. Please seed the database first.");
        }

        return Ok(vendor);
    }
}
