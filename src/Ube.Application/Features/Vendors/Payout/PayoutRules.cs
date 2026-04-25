
namespace Ube.Application.Features.Vendors.Payout;
public static class VendorPayoutRules
{
    public static Result ValidateAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return Result.Failure("Account number is required");

        if (accountNumber.Length < 8)
            return Result.Failure("Account number too short");

        if (accountNumber.Length > 20)
            return Result.Failure("Account number too long");

        return Result.Success();
    }
}