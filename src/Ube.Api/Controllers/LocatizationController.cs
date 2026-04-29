using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Features.Localization;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Common.Exceptions;

namespace Ube.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/settings/localization")]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _service;
    private readonly ICurrentUserService _currentUser;
    public LocalizationController(ILocalizationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // GET setting
    [HttpGet]
    public async Task<IActionResult> GetLocalization()
    {
        var userId = _currentUser.UserId;
        var result = await _service.GetLocalizationAsync(userId);
        if (result == null)
        {
            throw new NotFoundException("Localization settings not found");
        }
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateLocalization(UpdateLocalizationDto dto)
    {
        var userId = _currentUser.UserId;
        await _service.UpdateLocalizationAsync(userId, dto);
        return Ok(new { message = "Localization settings updated successfully" });
    }
}