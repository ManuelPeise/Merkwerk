using Microsoft.Extensions.Options;

namespace Logic.Authentication.Tests;

internal static class TestSettings
{
    public static readonly DateTimeOffset Start = new(2026, 10, 3, 8, 0, 0, TimeSpan.Zero);

    public const string SigningKey = "test-signing-key-with-at-least-32-bytes!";

    public static IOptions<JwtOptions> Jwt(string signingKey = SigningKey) => Options.Create(new JwtOptions
    {
        Issuer = "merkwerk-test",
        Audience = "merkwerk-test",
        SigningKey = signingKey,
        AccessTokenMinutes = 15,
        RefreshTokenDays = 14,
    });
}
