
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Application.Common.Exceptions;
using Ube.Application.Features.Vendors.Payout;
public class VendorPayoutService
{
    private readonly IVendorPayoutRepository _repo;
    private readonly IVendorProfileRepository _vendorRepo;

    public VendorPayoutService(
        IVendorPayoutRepository repo,
        IVendorProfileRepository vendorRepo)
    {
        _repo = repo;
        _vendorRepo = vendorRepo;
    }

    public async Task<VendorPayoutDto> GetAsync(Guid userId)
    {
        var vendor = await _vendorRepo.GetVendorIdAsync(userId);

        if (vendor == null)
            throw new NotFoundException("Vendor not found");

        var payout = await _repo.GetByVendorIdAsync(vendor.Id);

        if (payout == null)
            throw new NotFoundException("Payout details not found");

        return new VendorPayoutDto
        {
            BankName = payout.BankName,
            AccountNumber = payout.AccountNumber,
            AccountHolderName = payout.AccountHolderName,
            Branch = payout.Branch
        };
    }

    public async Task UpdateAsync(Guid userId, UpdateVendorPayoutDto dto)
    {
        var vendor = await _vendorRepo.GetVendorIdAsync(userId);

        if (vendor == null)
            throw new NotFoundException("Vendor not found");

        var payout = await _repo.GetByVendorIdAsync(vendor.Id);

        if (payout == null)
        {
            payout = new VendorPayout
            {
                Id = Guid.NewGuid(),
                VendorProfileId = vendor.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(payout);
        }

        payout.BankName = dto.BankName;
        payout.AccountNumber = dto.AccountNumber;
        payout.AccountHolderName = dto.AccountHolderName;
        payout.Branch = dto.Branch;
        payout.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(payout);
    }
}