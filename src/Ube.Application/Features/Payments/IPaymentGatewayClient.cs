namespace Ube.Application.Features.Payments;

public enum GatewayChargeStatus { Pending, Succeeded, Failed }

public record GatewayChargeResult(string ExternalReference, GatewayChargeStatus Status);
public record GatewayRefundResult(string ExternalReference, bool Succeeded);

// Minimal, generic boundary so any real gateway (Stripe/PayHere/etc,
// supplied by a separate team later) can implement it without touching
// domain/ledger code. idempotencyKey is always Payment.Id so retries never
// double-charge.
public interface IPaymentGatewayClient
{
    Task<GatewayChargeResult> InitiateChargeAsync(decimal amount, string currency, string idempotencyKey, string reference, CancellationToken ct = default);
    Task<GatewayChargeStatus> ConfirmChargeAsync(string externalReference, CancellationToken ct = default);
    Task<GatewayRefundResult> InitiateRefundAsync(string externalChargeReference, decimal amount, string idempotencyKey, CancellationToken ct = default);
    bool VerifyWebhookSignature(string payload, string signatureHeader);
}
