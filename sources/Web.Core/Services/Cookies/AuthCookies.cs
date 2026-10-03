namespace Web.Core.Services.Cookies;

/// <summary>Names and paths of the authentication cookies (ADR 013).</summary>
public static class AuthCookies
{
    /// <summary>Access token (JWT). Sent with every request to the site.</summary>
    public const string AccessToken = "mw_access";

    /// <summary>Refresh token. Only sent to the authentication endpoints.</summary>
    public const string RefreshToken = "mw_refresh";

    /// <summary>
    /// Route of AuthenticationController. Cookie paths are case-sensitive, so this relies on lowercase URLs
    /// (RouteOptions.LowercaseUrls) and on the client calling exactly this path.
    /// </summary>
    public const string RefreshTokenPath = "/api/v1/authentication";
}
