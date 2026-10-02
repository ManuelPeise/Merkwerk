namespace Logic.Authentication;

/// <summary>Settings for issuing and validating access tokens (section "Auth:Jwt").</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; set; } = "merkwerk";

    public string Audience { get; set; } = "merkwerk";

    /// <summary>HMAC key, at least 32 bytes. Comes from user secrets / environment, never from appsettings.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 14;
}
