using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Features.Vendors.Payout;

public interface IVendorPayoutRepository
{
    Task<VendorPayout?> GetByVendorIdAsync(Guid vendorId);
    Task AddAsync(VendorPayout payout);
    Task UpdateAsync(VendorPayout payout);
}