using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ube.Infrastructure.Persistence;

namespace Ube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();

        return Ok(categories);
    }
}
