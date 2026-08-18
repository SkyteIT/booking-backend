using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Interfaces.Persistence;
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
    private readonly IBookingRepository _bookingRepo;
    private readonly IPayoutBatchService _payoutBatchService;
    private readonly IVendorInvoiceService _invoiceService;
    private readonly IVendorCommissionAcknowledgementService _acknowledgementService;
    private readonly IPaymentDisputeService _disputeService;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly ICurrentUserService _currentUser;

    public VendorLedgerController(
        ILedgerRepository ledgerRepo,
        IBookingRepository bookingRepo,
        IPayoutBatchService payoutBatchService,
        IVendorInvoiceService invoiceService,
        IVendorCommissionAcknowledgementService acknowledgementService,
        IPaymentDisputeService disputeService,
        IVendorProfileRepository vendorRepo,
        ICurrentUserService currentUser)
    {
        _ledgerRepo = ledgerRepo;
        _bookingRepo = bookingRepo;
        _payoutBatchService = payoutBatchService;
        _invoiceService = invoiceService;
        _acknowledgementService = acknowledgementService;
        _disputeService = disputeService;
        _vendorRepo = vendorRepo;
        _currentUser = currentUser;
    }

    [HttpGet("disputes")]
    public async Task<IActionResult> GetDisputes(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var result = await _disputeService.GetForVendorAsync(vendorProfileId, ct);
        return Ok(result);
    }

    [HttpPost("commission-acknowledgement")]
    public async Task<IActionResult> AcknowledgeCommissionRate(AcknowledgeCommissionRateRequest request, CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var result = await _acknowledgementService.AcknowledgeAsync(vendorProfileId, request, ct);
        return Ok(result);
    }

    [HttpGet("commission-acknowledgements")]
    public async Task<IActionResult> GetCommissionAcknowledgements(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var result = await _acknowledgementService.GetHistoryAsync(vendorProfileId, ct);
        return Ok(result);
    }

    [HttpGet("commission-invoices")]
    public async Task<IActionResult> GetInvoices(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var invoices = await _invoiceService.GetForVendorAsync(vendorProfileId, ct);
        return Ok(invoices);
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> GetLedger(CancellationToken ct)
    {
        var vendorProfileId = await ResolveVendorProfileIdAsync();
        var entries = await _ledgerRepo.GetByVendorIdAsync(vendorProfileId, ct);

        // One grouped lookup for every entry's BookingNumber, not one
        // query per row - a vendor can't recognize their own booking by
        // its raw guid.
        var bookingIds = entries.Where(e => e.BookingId.HasValue).Select(e => e.BookingId!.Value);
        var bookingNumbers = await _bookingRepo.GetBookingNumbersByIdsAsync(bookingIds, ct);

        var result = entries.Select(e => new LedgerEntryDto
        {
            Id = e.Id,
            AccountType = e.AccountType,
            VendorProfileId = e.VendorProfileId,
            EntryType = e.EntryType,
            Direction = e.Direction,
            Amount = e.Amount,
            BookingId = e.BookingId,
            BookingNumber = e.BookingId.HasValue ? bookingNumbers.GetValueOrDefault(e.BookingId.Value) : null,
            PaymentId = e.PaymentId,
            RefundId = e.RefundId,
            PayoutBatchId = e.PayoutBatchId,
            VendorCommissionInvoiceId = e.VendorCommissionInvoiceId,
            CreatedAt = e.CreatedAt
        });

        return Ok(result);
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
