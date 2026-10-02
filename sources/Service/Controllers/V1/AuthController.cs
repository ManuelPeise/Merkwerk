using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Auth;
using Shared.Api.Auth;

namespace Service.Controllers.V1;

/// <summary>
/// Login, refresh and logout with the JWT in an HttpOnly cookie (ADR 013).
/// Only transport lives here (cookies, status codes); the session logic is in <see cref="IAuthSessionService"/>.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthSessionService sessions) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionInfo>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var session = await sessions.LoginAsync(request.Email, request.Password, cancellationToken);

        return session is null ? Unauthorized() : WriteSession(session);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionInfo>> Refresh(CancellationToken cancellationToken)
    {
        var session = Request.Cookies.TryGetValue(AuthCookies.RefreshToken, out var token)
            ? await sessions.RefreshAsync(token, cancellationToken)
            : null;

        if (session is null)
        {
            ClearCookies();
            return Unauthorized();
        }

        return WriteSession(session);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AuthCookies.RefreshToken, out var token))
        {
            await sessions.LogoutAsync(token, cancellationToken);
        }

        ClearCookies();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<SessionInfo> Me()
    {
        var expires = long.TryParse(User.FindFirst("exp")?.Value, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : DateTimeOffset.MinValue;

        return new SessionInfo(User.Identity?.Name ?? "?", User.FindFirst("role")?.Value ?? "?", expires);
    }

    private SessionInfo WriteSession(AuthSession session)
    {
        Response.Cookies.Append(AuthCookies.AccessToken, session.AccessToken, CookieOptions("/", session.AccessTokenExpiresAt));
        Response.Cookies.Append(
            AuthCookies.RefreshToken,
            session.RefreshToken,
            CookieOptions(AuthCookies.RefreshTokenPath, session.RefreshTokenExpiresAt));

        return new SessionInfo(session.Name, session.Role, session.AccessTokenExpiresAt);
    }

    private void ClearCookies()
    {
        Response.Cookies.Delete(AuthCookies.AccessToken, CookieOptions("/", null));
        Response.Cookies.Delete(AuthCookies.RefreshToken, CookieOptions(AuthCookies.RefreshTokenPath, null));
    }

    private static CookieOptions CookieOptions(string path, DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = path,
        Expires = expires,
        IsEssential = true,
    };
}
