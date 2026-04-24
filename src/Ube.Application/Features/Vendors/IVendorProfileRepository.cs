using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorProfileRepository
{
    Task<VendorProfile?> GetVendorIdAsync(Guid userId);
    Task UpdateAsync(VendorProfile profile);
}