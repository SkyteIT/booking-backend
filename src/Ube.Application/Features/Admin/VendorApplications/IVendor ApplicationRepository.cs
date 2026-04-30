using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorApplicationRepository
{
    Task<VendorApplication?> GetByIdAsync(Guid id);
    Task UpdateAsync(VendorApplication application);
    Task <List<VendorApplication>> GetAllAsync();
}