namespace Ube.Application.Features.Vendors.Payout;

public interface IVendorPayoutService
{
    Task<VendorPayoutDto> GetAsync(Guid userId);
    Task UpdateAsync(Guid userId, UpdateVendorPayoutDto dto);
}
