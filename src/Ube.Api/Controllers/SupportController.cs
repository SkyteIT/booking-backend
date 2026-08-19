using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Support;

namespace Ube.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/support")]
public class SupportController : ControllerBase
{
    private readonly ISupportService _service;
    private readonly ICurrentUserService _currentUser;

    public SupportController(ISupportService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> SubmitTicket([FromBody] SubmitSupportTicketDto dto, CancellationToken ct)
    {
        await _service.SubmitTicketAsync(_currentUser.UserId, dto, ct);
        return Ok(new { message = "Support ticket submitted" });
    }
}
