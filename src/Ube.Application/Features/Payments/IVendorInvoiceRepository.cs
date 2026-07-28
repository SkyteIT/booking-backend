using Ube.Domain.Entities.Payments;

namespace Ube.Application.Features.Payments;

public interface IVendorInvoiceRepository
{
    Task<VendorCommissionInvoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<VendorCommissionInvoice>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default);
    Task<IReadOnlyList<VendorCommissionInvoice>> GetOverdueCandidatesAsync(DateTime asOf, CancellationToken ct = default);
    Task AddAsync(VendorCommissionInvoice invoice, CancellationToken ct = default);
    Task UpdateAsync(VendorCommissionInvoice invoice, CancellationToken ct = default);
}
