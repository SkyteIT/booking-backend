namespace Ube.Application.Common.Helpers;
public static class MaskingHelper
{
    public static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber))
            return string.Empty;

        if (accountNumber.Length <= 4)
            return "****";

        var last4 = accountNumber[^4..];
        return new string('*', accountNumber.Length - 4) + last4;
    }
}