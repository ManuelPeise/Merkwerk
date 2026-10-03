namespace Web.Core.Services.Cookies;

/// <summary>Names and paths of the authentication cookies (ADR 013).</summary>
public static class AuthCookies
{
    /// <summary>Access token (JWT). Sent with every request to the site.</summary>
    public const string AccessToken = "mw_access";

    /// <summary>Refresh token (adults and children). Only sent to the authentication endpoints.</summary>
    public const string RefreshToken = "mw_refresh";

    /// <summary>
    /// Route of AuthenticationController. Cookie paths are case-sensitive, so this relies on lowercase URLs
    /// (RouteOptions.LowercaseUrls) and on the client calling exactly this path.
    /// </summary>
    public const string RefreshTokenPath = "/api/v1/authentication";

    /// <summary>Device token of a paired device (LP-106). Only sent to DevicesController.</summary>
    public const string DeviceToken = "mw_device";

    /// <summary>Route of DevicesController (lowercase, see <see cref="RefreshTokenPath"/>).</summary>
    public const string DeviceTokenPath = "/api/v1/devices";
}
