using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;

namespace Ube.Api.Controllers.Payments;

// Admin can view invoices (customer support needs the context), but every
// action that computes/collects/enforces money owed is Finance-only.
[ApiController]
[Authorize(Roles = "Admin,Finance")]
[Route("api/vendor-invoices")]
public class VendorInvoicesController : ControllerBase
{
    private readonly IVendorInvoiceService _invoiceService;
    private readonly ICurrentUserService _currentUser;

    public VendorInvoicesController(IVendorInvoiceService invoiceService, ICurrentUserService currentUser)
    {
        _invoiceService = invoiceService;
        _currentUser = currentUser;
    }

    [HttpPost("compute")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> Compute(ComputeVendorInvoiceRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.ComputeAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-paid")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> MarkPaid(Guid id, CancellationToken ct)
    {
        var result = await _invoiceService.MarkPaidAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/mark-overdue")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> MarkOverdue(Guid id, CancellationToken ct)
    {
        var result = await _invoiceService.MarkOverdueAsync(_currentUser.UserId, id, ct);
        return Ok(result);
    }

    [HttpPost("process-overdue")]
    [Authorize(Roles = "Finance")]
    public async Task<IActionResult> ProcessOverdue(CancellationToken ct)
    {
        var result = await _invoiceService.ProcessOverdueAsync(_currentUser.UserId, ct);
        return Ok(result);
    }

    [HttpGet("vendor/{vendorProfileId:guid}")]
    public async Task<IActionResult> GetForVendor(Guid vendorProfileId, CancellationToken ct)
    {
        var result = await _invoiceService.GetForVendorAsync(vendorProfileId, ct);
        return Ok(result);
    }
}
