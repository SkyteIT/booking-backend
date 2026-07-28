namespace Ube.Application.Features.Payments;

public interface IVendorInvoiceService
{
    Task<VendorInvoiceDto> ComputeAsync(ComputeVendorInvoiceRequest request, CancellationToken ct = default);
    Task<VendorInvoiceDto> MarkPaidAsync(Guid actorUserId, Guid invoiceId, CancellationToken ct = default);
    Task<VendorInvoiceDto> MarkOverdueAsync(Guid actorUserId, Guid invoiceId, CancellationToken ct = default);

    // Bulk sweep over every invoice past its DueDate - there's no
    // scheduler in this codebase yet, so an admin (or a cron job added
    // later) triggers this explicitly rather than it running automatically.
    Task<IReadOnlyList<VendorInvoiceDto>> ProcessOverdueAsync(Guid actorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<VendorInvoiceDto>> GetForVendorAsync(Guid vendorProfileId, CancellationToken ct = default);
}
