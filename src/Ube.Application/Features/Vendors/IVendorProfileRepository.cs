using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorProfileRepository
{
    Task<VendorProfile?> GetVendorIdAsync(Guid userId);
    Task<VendorProfile?> GetByIdAsync(Guid vendorProfileId);
    Task<IReadOnlyList<VendorProfile>> GetAllAsync();
    Task UpdateAsync(VendorProfile profile);
    Task AddAsync(VendorProfile profile);
}