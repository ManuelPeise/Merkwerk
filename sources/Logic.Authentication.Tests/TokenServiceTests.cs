using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Logic.Authentication.Tests;

public sealed class TokenServiceTests
{
    private readonly JsonWebTokenHandler _handler = new();

    [Fact]
    public async Task CreateAccessToken_ValidSettings_ContainsSubjectNameAndRole()
    {
        // Arrange
        var service = new TokenService(TestSettings.Jwt(), new ManualTimeProvider(TestSettings.Start));

        // Act
        var (token, _) = service.CreateAccessToken("user-7", "anna@example.org", "OrgAdmin");
        var result = await _handler.ValidateTokenAsync(token, CreateValidationParameters(TestSettings.SigningKey));

        // Assert
        Assert.True(result.IsValid, result.Exception?.Message);
        Assert.Equal("user-7", result.ClaimsIdentity.FindFirst("sub")?.Value);
        Assert.Equal("anna@example.org", result.ClaimsIdentity.FindFirst("name")?.Value);
        Assert.Equal("OrgAdmin", result.ClaimsIdentity.FindFirst("role")?.Value);
    }

    [Fact]
    public void CreateAccessToken_ConfiguredLifetime_ExpiresAfterAccessTokenMinutes()
    {
        var service = new TokenService(TestSettings.Jwt(), new ManualTimeProvider(TestSettings.Start));

        var (token, expiresAt) = service.CreateAccessToken("user-7", "anna@example.org", "OrgAdmin");

        Assert.Equal(TestSettings.Start.AddMinutes(15), expiresAt);
        Assert.Equal(expiresAt.UtcDateTime, _handler.ReadJsonWebToken(token).ValidTo);
    }

    [Fact]
    public async Task CreateAccessToken_ValidatedWithOtherKey_IsRejected()
    {
        var service = new TokenService(TestSettings.Jwt(), new ManualTimeProvider(TestSettings.Start));

        var (token, _) = service.CreateAccessToken("user-7", "anna@example.org", "OrgAdmin");
        var result = await _handler.ValidateTokenAsync(
            token,
            CreateValidationParameters("another-signing-key-with-at-least-32-bytes"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateRefreshToken_TwoCalls_ReturnsDifferent32ByteTokens()
    {
        var first = TokenService.CreateRefreshToken();
        var second = TokenService.CreateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.Equal(32, Convert.FromBase64String(first).Length);
    }

    private static TokenValidationParameters CreateValidationParameters(string signingKey) => new()
    {
        ValidIssuer = "merkwerk-test",
        ValidAudience = "merkwerk-test",
        IssuerSigningKey = TokenService.CreateSigningKey(new JwtOptions { SigningKey = signingKey }),
        // The test clock is fixed in the past; lifetime is checked separately via ValidTo.
        ValidateLifetime = false,
    };
}
