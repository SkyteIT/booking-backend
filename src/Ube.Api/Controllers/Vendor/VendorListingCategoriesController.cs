using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers.Vendor;

[ApiController]
[Authorize(Roles = "Vendor")]
[Route("api/vendor/listing-categories")]
public class VendorListingCategoriesController(
    IVendorListingCategoryService categories,
    ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await categories.GetAllowedAsync(currentUser.UserId, ct));
}
