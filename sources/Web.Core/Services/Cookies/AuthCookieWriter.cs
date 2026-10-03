using Logic.Authentication;

namespace Web.Core.Services.Cookies;

/// <summary>Writes and deletes the HttpOnly auth cookies (transport only, ADR 013).</summary>
public sealed class AuthCookieWriter(IWebHostEnvironment environment)
{
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

    private CookieOptions CreateOptions(HttpContext context, string path, DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        // Production always runs behind TLS (Caddy). Development runs over plain HTTP (localhost, phones on the LAN),
        // where browsers would drop Secure cookies.
        Secure = context.Request.IsHttps || !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = path,
        Expires = expires,
        IsEssential = true,
    };
}
