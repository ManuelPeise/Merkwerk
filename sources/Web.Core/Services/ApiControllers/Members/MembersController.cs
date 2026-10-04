using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Web.Core.Services.ApiControllers.Members.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Members;

/// <summary>Adults of the family (LP-105).</summary>
public sealed class MembersController : ApiControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    /// <summary>GET /api/v1/members/list</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MemberDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _memberService.ListAsync(organizationId, userId, cancellationToken) is not { } members)
        {
            return Forbid();
        }

        return members.Select(MemberDto.From).ToList();
    }

    /// <summary>POST /api/v1/members/remove – the owner and the acting admin cannot be removed (409).</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveAsync(RemoveMemberRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        return await _memberService.RemoveAsync(organizationId, userId, request.MembershipId, cancellationToken) switch
        {
            RemoveMemberStatus.Success => NoContent(),
            RemoveMemberStatus.IsOwner => Problem(statusCode: StatusCodes.Status409Conflict, title: "The owner cannot be removed"),
            RemoveMemberStatus.IsSelf => Problem(statusCode: StatusCodes.Status409Conflict, title: "You cannot remove yourself"),
            _ => NotFound(),
        };
    }
}
