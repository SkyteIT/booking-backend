using Microsoft.EntityFrameworkCore;
using Ube.Application.Features.Payments;
using Ube.Domain.Entities.Payments;
using Ube.Domain.Enums.Payments;

namespace Ube.Infrastructure.Persistence.Repositories.Payments;

public class VendorInvoiceRepository : IVendorInvoiceRepository
{
    private readonly ApplicationDbContext _db;

    public VendorInvoiceRepository(ApplicationDbContext db) => _db = db;

    public async Task<VendorCommissionInvoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.VendorCommissionInvoices.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<VendorCommissionInvoice>> GetByVendorIdAsync(Guid vendorProfileId, CancellationToken ct = default)
        => await _db.VendorCommissionInvoices
            .Where(x => x.VendorProfileId == vendorProfileId)
            .OrderByDescending(x => x.PeriodEnd)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VendorCommissionInvoice>> GetOverdueCandidatesAsync(DateTime asOf, CancellationToken ct = default)
        => await _db.VendorCommissionInvoices
            .Where(x => x.Status == VendorInvoiceStatus.Pending && x.DueDate < asOf)
            .ToListAsync(ct);

    public async Task AddAsync(VendorCommissionInvoice invoice, CancellationToken ct = default)
    {
        await _db.VendorCommissionInvoices.AddAsync(invoice, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VendorCommissionInvoice invoice, CancellationToken ct = default)
    {
        _db.VendorCommissionInvoices.Update(invoice);
        await _db.SaveChangesAsync(ct);
    }
}
