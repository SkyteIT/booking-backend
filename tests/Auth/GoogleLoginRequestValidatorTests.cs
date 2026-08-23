using Ube.Application.Features.Auth;
using Ube.Application.Features.Auth.Validators;

namespace tests.Auth;

public class GoogleLoginRequestValidatorTests
{
    private readonly GoogleLoginRequestValidator _validator = new();

    [Fact]
    public void AcceptsLegacyIdTokenPayload()
    {
        var result = _validator.Validate(new GoogleLoginRequest { IdToken = "id-token" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AcceptsGoogleIdentityServicesCredentialPayload()
    {
        var result = _validator.Validate(new GoogleLoginRequest { Credential = "credential-token" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsRequestWithoutEitherTokenShape()
    {
        var result = _validator.Validate(new GoogleLoginRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Google ID token is required");
    }
}
