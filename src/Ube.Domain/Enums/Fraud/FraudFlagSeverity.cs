namespace Ube.Domain.Enums.Fraud;

// FlagOnly: the booking proceeds normally, this is pattern-tracking only.
// Hold: the booking is created Pending with payment capture deferred until
// an admin reviews it - only FraudRuleType.NewAccountHighValue produces this.
public enum FraudFlagSeverity
{
    FlagOnly = 1,
    Hold = 2
}
