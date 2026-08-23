namespace Ube.Application.Features.Auth;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;

    // Google Identity Services names the returned ID token "credential".
    // Keep IdToken for existing clients and accept the native GIS response too.
    public string Credential { get; set; } = string.Empty;

    public string? DeviceToken { get; set; }

    public string EffectiveIdToken => !string.IsNullOrWhiteSpace(IdToken)
        ? IdToken
        : Credential;
}
