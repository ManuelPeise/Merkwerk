using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Organizations;
using Web.Core.Services.ApiControllers.Authentication.Dtos;
using Web.Core.Services.ApiControllers.Setup.Dtos;
using Web.Core.Services.Cookies;

namespace Web.Core.Services.ApiControllers.Setup;

/// <summary>First-run setup (LP-105): only usable while the instance has no family.</summary>
public sealed class SetupController(ISetupService setupService, AuthCookieWriter cookieWriter) : ApiControllerBase
{
    /// <summary>GET /api/v1/setup/status – the web client redirects to /setup while this is true.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<SetupStatusDto>(StatusCodes.Status200OK)]
    public async Task<SetupStatusDto> StatusAsync(CancellationToken cancellationToken) =>
        new(await setupService.IsSetupRequiredAsync(cancellationToken));

    /// <summary>POST /api/v1/setup/initialize – creates owner and family and signs the owner in.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> InitializeAsync(
        SetupInitializeRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await setupService.InitializeAsync(
            new SetupRequest(request.FamilyName, request.DisplayName, request.Email, request.Password, request.PrivacyAccepted),
            cancellationToken);

        switch (result.Status)
        {
            case SetupStatus.Success:
                cookieWriter.Write(Response, result.Session!);
                return SessionDto.From(result.Session!);
            case SetupStatus.AlreadyDone:
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Already set up");
            default:
                return FieldErrors(result.Errors);
        }
    }
}
