using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Features.Vendors;

public interface IVendorProfileService
{
    Task<VendorProfileDto> GetVendorProfileAsync(Guid userId);

    Task<VendorProfileDto> UpdateVendorProfileAsync(Guid userId, UpdateVendorProfileDto Dto);
    Task UpdateProfileImageAsync(Guid userId, string imageUrl);
}