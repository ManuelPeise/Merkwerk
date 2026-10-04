using Shared.Models.Authentication;

namespace Web.Core.Services.Cookies;

/// <summary>Writes and deletes the HttpOnly auth cookies (transport only, ADR 013).</summary>
public sealed class AuthCookieWriter
{
    private readonly IWebHostEnvironment _environment;

    public AuthCookieWriter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void Write(HttpResponse response, AuthSession session)
    {
        response.Cookies.Append(
            AuthCookies.AccessToken,
            session.AccessToken,
            CreateOptions(response.HttpContext, "/", session.AccessTokenExpiresAt));

        response.Cookies.Append(
            AuthCookies.RefreshToken,
            session.RefreshToken,
            CreateOptions(response.HttpContext, AuthCookies.RefreshTokenPath, session.RefreshTokenExpiresAt));
    }

    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(AuthCookies.AccessToken, CreateOptions(response.HttpContext, "/", expires: null));
        response.Cookies.Delete(AuthCookies.RefreshToken, CreateOptions(response.HttpContext, AuthCookies.RefreshTokenPath, expires: null));
    }

    /// <summary>Device token of a paired device (LP-106); renewed with every child sign-in.</summary>
    public void WriteDevice(HttpResponse response, string deviceToken, DateTimeOffset expiresAt) =>
        response.Cookies.Append(
            AuthCookies.DeviceToken,
            deviceToken,
            CreateOptions(response.HttpContext, AuthCookies.DeviceTokenPath, expiresAt));

    public void DeleteDevice(HttpResponse response) =>
        response.Cookies.Delete(AuthCookies.DeviceToken, CreateOptions(response.HttpContext, AuthCookies.DeviceTokenPath, expires: null));

    private CookieOptions CreateOptions(HttpContext context, string path, DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        // Production always runs behind TLS (Caddy). Development runs over plain HTTP (localhost, phones on the LAN),
        // where browsers would drop Secure cookies.
        Secure = context.Request.IsHttps || !_environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = path,
        Expires = expires,
        IsEssential = true,
    };
}
