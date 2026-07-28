using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Payments;
using Ube.Application.Features.Vendors;

namespace Ube.Api.Controllers.Payments;

[ApiController]
[Authorize(Roles = "Vendor")]
[Route("api/vendor")]
public class VendorLedgerController : ControllerBase
{
    private readonly ILedgerRepository _ledgerRepo;
    private readonly IPayoutBatchService _payoutBatchService;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly ICurrentUserService _currentUser;

    public VendorLedgerController(
        ILedgerRepository ledgerRepo,
        IPayoutBatchService payoutBatchService,
        IVendorProfileRepository vendorRepo,
        ICurrentUserService currentUser)
    {
        _ledgerRepo = ledgerRepo;
        _payoutBatchService = payoutBatchService;
        _vendorRepo = vendorRepo;
        _currentUser = currentUser;
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var entries = await _ledgerRepo.GetByVendorIdAsync(vendorProfileId, ct);
        return Ok(entries);
    }

    [HttpGet("payout-batches")]
    public async Task<IActionResult> GetPayoutBatches(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var batches = await _payoutBatchService.GetForVendorAsync(vendorProfileId, ct);
        return Ok(batches);
    }

    private async Task<Guid> ResolveVendorProfileIdAsync()
    {
        var profile = await _vendorRepo.GetVendorIdAsync(_currentUser.UserId)
            ?? throw new NotFoundException("Vendor profile not found");
        return profile.Id;
    }
}
