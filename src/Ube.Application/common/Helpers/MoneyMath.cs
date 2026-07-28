namespace Ube.Application.Common.Helpers;

// Single place for all money rounding so CommissionAmount + PlatformFeeAmount
// + NetVendorAmount == Amount holds exactly, every time. Never compute this
// inline at a call site - route it through here.
public static class MoneyMath
{
    public static decimal RoundCurrency(decimal amount)
        => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static decimal PercentOf(decimal amount, decimal percent)
        => RoundCurrency(amount * percent / 100m);

    public record CommissionSplit(decimal CommissionAmount, decimal PlatformFeeAmount, decimal NetVendorAmount);

    public static CommissionSplit SplitCommission(decimal grossAmount, decimal commissionPercent, decimal? flatPlatformFee)
    {
        var commission = PercentOf(grossAmount, commissionPercent);
        var platformFee = flatPlatformFee.HasValue ? RoundCurrency(flatPlatformFee.Value) : 0m;
        var netVendor = grossAmount - commission - platformFee;
        return new CommissionSplit(commission, platformFee, netVendor);
    }
}
