using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Content.Banner;
using Ube.Application.Features.Content.Category;
using Ube.Application.Features.Content.Promotion;

namespace Ube.Api.Controllers.Admin;

[Authorize(Roles = "Admin,SuperAdmin")]
[ApiController]
[Route("api/admin/content-management")]
public class AdminContentManagementController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IBannerService _bannerService;
    private readonly IPromotionService _promotionService;

    public AdminContentManagementController(
        ICategoryService categoryService,
        IBannerService bannerService,
        IPromotionService promotionService)
    {
        _categoryService = categoryService;
        _bannerService = bannerService;
        _promotionService = promotionService;
    }

    [HttpGet]
    [HttpGet("overview")]
    [HttpGet("details")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var categoriesTask = _categoryService.GetAllAsync(cancellationToken);
        var bannersTask = _bannerService.GetAllAsync(cancellationToken);
        var promotionsTask = _promotionService.GetAllAsync(cancellationToken);

        await Task.WhenAll(categoriesTask, bannersTask, promotionsTask);

        return Ok(new
        {
            categories = await categoriesTask,
            banners = await bannersTask,
            promotions = await promotionsTask
        });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("banners")]
    public async Task<IActionResult> GetBanners(CancellationToken cancellationToken)
    {
        var result = await _bannerService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("banners/{id:guid}")]
    public async Task<IActionResult> GetBannerById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bannerService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions(CancellationToken cancellationToken)
    {
        var result = await _promotionService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("promotions/{id:guid}")]
    public async Task<IActionResult> GetPromotionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _promotionService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
