using Data.Database.Entities.Organizations;
using Logic.Organizations.Invitations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Core.Services.ApiControllers.Authentication.Dtos;
using Web.Core.Services.ApiControllers.Invitations.Dtos;
using Web.Core.Services.Authorization;
using Web.Core.Services.Cookies;

namespace Web.Core.Services.ApiControllers.Invitations;

/// <summary>Invitations of adults into the family (LP-105).</summary>
public sealed class InvitationsController(IInvitationService invitationService, AuthCookieWriter cookieWriter)
    : ApiControllerBase
{
    /// <summary>POST /api/v1/invitations/create – mails an invitation link (7 days, single use).</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<InvitationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvitationDto>> CreateAsync(
        CreateInvitationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await invitationService.CreateAsync(
            organizationId, userId, request.Email, Enum.Parse<OrganizationRole>(request.Role), MailLanguage, cancellationToken);

        return result.Status switch
        {
            CreateInvitationStatus.Created => InvitationDto.From(result.Invitation!),
            CreateInvitationStatus.AlreadyMember => Problem(statusCode: StatusCodes.Status409Conflict, title: "Already a member"),
            _ => Forbid(),
        };
    }

    /// <summary>GET /api/v1/invitations/list – open invitations of the family.</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvitationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<InvitationDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await invitationService.ListAsync(organizationId, userId, cancellationToken) is not { } invitations)
        {
            return Forbid();
        }

        return invitations.Select(InvitationDto.From).ToList();
    }

    /// <summary>POST /api/v1/invitations/revoke – withdraws an open invitation.</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(RevokeInvitationRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        return await invitationService.RevokeAsync(organizationId, userId, request.Id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    /// <summary>GET /api/v1/invitations/details?token=… – what the invitation page shows before accepting.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<InvitationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status410Gone)]
    public async Task<ActionResult<InvitationDetailsDto>> DetailsAsync(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var result = await invitationService.GetDetailsAsync(token, cancellationToken);

        return result.Status switch
        {
            InvitationLookupStatus.Found => InvitationDetailsDto.From(result.Details!),
            InvitationLookupStatus.Gone => Problem(statusCode: StatusCodes.Status410Gone, title: "Invitation no longer valid"),
            _ => NotFound(),
        };
    }

    /// <summary>
    /// POST /api/v1/invitations/accept – anonymous with a new account, or signed in with only the token (adds the
    /// membership to the current user). Signs in and sets the cookies.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status410Gone)]
    public async Task<ActionResult<SessionDto>> AcceptAsync(
        AcceptInvitationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await invitationService.AcceptAsync(
            new AcceptInvitationRequest(request.Token, request.DisplayName, request.Password, request.PrivacyAccepted),
            CurrentUserId,
            cancellationToken);

        switch (result.Status)
        {
            case AcceptInvitationStatus.Success:
                cookieWriter.Write(Response, result.Session!);
                return SessionDto.From(result.Session!);
            case AcceptInvitationStatus.Invalid:
                return FieldErrors(result.Errors!);
            case AcceptInvitationStatus.AccountExists:
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Account exists");
            case AcceptInvitationStatus.EmailMismatch:
                return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Invitation is for another address");
            case AcceptInvitationStatus.Gone:
                return Problem(statusCode: StatusCodes.Status410Gone, title: "Invitation no longer valid");
            default:
                return NotFound();
        }
    }
}
