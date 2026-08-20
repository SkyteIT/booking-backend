
using Ube.Application.Features.Vendors;
using Ube.Domain.Entities.Vendors;
using Ube.Application.Common.Exceptions;
using Ube.Application.Common.Helpers;
using Ube.Application.Features.Vendors.Payout;
using Ube.Application.Common.Interfaces.Services;
public class VendorPayoutService : IVendorPayoutService
{
    private readonly IVendorPayoutRepository _repo;
    private readonly IVendorProfileRepository _vendorRepo;
    private readonly IEncryptionService _encryptionService;

    public VendorPayoutService(
        IVendorPayoutRepository repo,
        IVendorProfileRepository vendorRepo,
        IEncryptionService encryptionService)
    {
        _repo = repo;
        _vendorRepo = vendorRepo;
        _encryptionService = encryptionService;
    }

    public async Task<VendorPayoutDto> GetAsync(Guid userId)
    {
        var vendor = await _vendorRepo.GetVendorIdAsync(userId);
        

        if (vendor == null)
            throw new NotFoundException("Vendor not found");

        var payout = await _repo.GetByVendorIdAsync(vendor.Id);

        if (payout == null)
            throw new NotFoundException("Payout details not found");

        var decryptedAccount = string.Empty;
        if (!string.IsNullOrEmpty(payout.AccountNumber))
        {
            try
            {
                decryptedAccount = _encryptionService.Decrypt(payout.AccountNumber);
            }
            catch
            {
                decryptedAccount = "****";
            }
        }

        return new VendorPayoutDto
        {
            BankName = SafeDecrypt(payout.BankName),
            AccountNumber = MaskingHelper.MaskAccountNumber(decryptedAccount),
            AccountHolderName = SafeDecrypt(payout.AccountHolderName),
            Branch = SafeDecrypt(payout.Branch)
        };
    }

    // Bank details are encrypted at rest (same policy as AccountNumber) -
    // a decrypt failure (e.g. a pre-encryption legacy row) falls back to
    // the raw stored value rather than throwing, since these three are
    // sometimes shown as-is (unlike AccountNumber, which is always masked).
    private string SafeDecrypt(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        try { return _encryptionService.Decrypt(value); }
        catch { return value; }
    }

    public async Task UpdateAsync(Guid userId, UpdateVendorPayoutDto dto)
    {
        var vendor = await _vendorRepo.GetVendorIdAsync(userId);

        if (vendor == null)
            throw new NotFoundException("Vendor not found");
        
        var result = VendorPayoutRules.ValidateAccountNumber(dto.AccountNumber);
        if (!result.IsSuccess)
            throw new ValidationException(result.Errors);

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

        payout.BankName = _encryptionService.Encrypt(dto.BankName);
        payout.AccountNumber = _encryptionService.Encrypt(dto.AccountNumber);
        payout.AccountHolderName = _encryptionService.Encrypt(dto.AccountHolderName);
        payout.Branch = _encryptionService.Encrypt(dto.Branch);
        payout.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(payout);
    }
}