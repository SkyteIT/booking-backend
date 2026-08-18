namespace Ube.Application.Features.Vendors.Payout;
public class VendorPayoutDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
}