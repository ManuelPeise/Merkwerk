using Logic.Authentication;
using Logic.Authentication.Accounts;
using Logic.Authentication.Sessions;
using Logic.Organizations.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Core.Services.ApiControllers.Authentication.Dtos;
using Web.Core.Services.Authorization;
using Web.Core.Services.Cookies;

namespace Web.Core.Services.ApiControllers.Authentication;

/// <summary>
/// Login, token refresh, logout and account operations (ADR 013, LP-104). Tokens travel only in HttpOnly cookies;
/// the logic lives in Logic.Authentication, this controller does transport only.
/// </summary>
public sealed class AuthenticationController(
    IAuthSessionService authSessionService,
    IAccountService accountService,
    IMemberService memberService,
    AuthCookieWriter cookieWriter) : ApiControllerBase
{
    /// <summary>POST /api/v1/authentication/login – checks the credentials and sets the auth cookies.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SessionDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authSessionService.LoginAsync(request.Email, request.Password, cancellationToken);

        switch (result.Status)
        {
            case LoginStatus.Success:
                cookieWriter.Write(Response, result.Session!);
                return SessionDto.From(result.Session!);
            case LoginStatus.EmailNotConfirmed:
                return Problem(statusCode: StatusCodes.Status403Forbidden, title: "E-mail not confirmed");
            default:
                // Same answer for unknown user, wrong password and locked account – don't reveal which one it was.
                return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials");
        }
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
    [Authorize(Policy = AuthorizationPolicies.PasswordChangeAllowed)]
    [HttpGet]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserDto> Me() => new CurrentUserDto(
        User.FindFirst(AuthClaims.Name)?.Value ?? string.Empty,
        User.FindFirst(AuthClaims.Role)?.Value ?? string.Empty,
        User.HasClaim(AuthClaims.MustChangePassword, "true"));

    /// <summary>POST /api/v1/authentication/forgot-password – mails a reset link if the account exists. Always 204.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        await accountService.RequestPasswordResetAsync(request.Email, MailLanguage, cancellationToken);
        return NoContent();
    }

    /// <summary>POST /api/v1/authentication/reset-password – sets the new password; ends all sessions of the user.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var result = await accountService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
        return result.Succeeded ? NoContent() : FieldProblem(nameof(request.NewPassword), result);
    }

    /// <summary>POST /api/v1/authentication/confirm-email – confirms the address from the link.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken)
    {
        var result = await accountService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        return result.Succeeded ? NoContent() : FieldProblem(nameof(request.Token), result);
    }

    /// <summary>
    /// POST /api/v1/authentication/change-password – also allowed while a start password is active.
    /// Ends all other sessions and sets new cookies for this one.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.PasswordChangeAllowed)]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionDto>> ChangePasswordAsync(
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await accountService.ChangePasswordAsync(
            userId, request.CurrentPassword, request.NewPassword, cancellationToken);

        if (!result.Succeeded)
        {
            return FieldProblem(nameof(request.NewPassword), result);
        }

        cookieWriter.Write(Response, result.Session!);
        return SessionDto.From(result.Session!);
    }

    /// <summary>
    /// POST /api/v1/authentication/admin-reset-password – an admin gives an adult of the same family a start password
    /// by mail (24 h, must be changed at the next login). 404 for anyone outside the family (LP-104/LP-105).
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminResetPasswordAsync(
        AdminResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return NotFound();
        }

        return await memberService.IssueStartPasswordAsync(
            organizationId, userId, request.UserId, MailLanguage, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    /// <summary>400 with Identity's messages attached to one field (camelCase, like the automatic model validation).</summary>
    private ActionResult FieldProblem(string field, AccountResult result)
    {
        var key = char.ToLowerInvariant(field[0]) + field[1..];

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(key, error);
        }

        return ValidationProblem(ModelState);
    }
}
