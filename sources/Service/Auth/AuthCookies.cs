namespace Service.Auth;

/// <summary>Names and paths of the authentication cookies (ADR 013).</summary>
public static class AuthCookies
{
    /// <summary>Access token (JWT). Sent with every request to the site.</summary>
    public const string AccessToken = "mw_access";

    /// <summary>Refresh token. Only sent to the auth endpoints.</summary>
    public const string RefreshToken = "mw_refresh";

    public const string RefreshTokenPath = "/api/v1/auth";
}
