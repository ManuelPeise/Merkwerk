using System.Security.Claims;
using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Core.Services.ApiControllers.Authentication.Dtos;
using Web.Core.Services.Cookies;

namespace Web.Core.Services.ApiControllers.Authentication;

/// <summary>
/// Login, token refresh and logout (ADR 013). Tokens travel only in HttpOnly cookies; the session logic
/// lives in Logic.Authentication, this controller does transport only.
/// </summary>
public sealed class AuthenticationController(
    IAuthSessionService authSessionService,
    AuthCookieWriter cookieWriter) : ApiControllerBase
{
    /// <summary>POST /api/v1/authentication/login – checks the credentials and sets the auth cookies.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var session = await authSessionService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (session is null)
        {
            // Same answer for unknown user and wrong password – don't reveal which one it was.
            return Unauthorized();
        }

        cookieWriter.Write(Response, session);
        return SessionDto.From(session);
    }

    /// <summary>
    /// POST /api/v1/authentication/refresh – redeems the refresh cookie (one-time use) and sets new cookies.
    /// Called by the web client automatically after a 401.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionDto>> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[AuthCookies.RefreshToken];
        var session = string.IsNullOrEmpty(refreshToken)
            ? null
            : await authSessionService.RefreshAsync(refreshToken, cancellationToken);

        if (session is null)
        {
            cookieWriter.Delete(Response);
            return Unauthorized();
        }

        cookieWriter.Write(Response, session);
        return SessionDto.From(session);
    }

    /// <summary>
    /// POST /api/v1/authentication/logout – revokes the refresh token and deletes the cookies.
    /// Anonymous on purpose: logging out must also work with an expired access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[AuthCookies.RefreshToken];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await authSessionService.LogoutAsync(refreshToken, cancellationToken);
        }

        cookieWriter.Delete(Response);
        return NoContent();
    }

    /// <summary>GET /api/v1/authentication/me – who is signed in? Lets the client restore its state after a reload.</summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserDto> Me() =>
        new CurrentUserDto(User.Identity?.Name ?? string.Empty, User.FindFirstValue("role") ?? string.Empty);
}
