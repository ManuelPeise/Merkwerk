using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Organizations;
using Web.Core.Services.ApiControllers.Groups.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Groups;

/// <summary>Groups of children (LP-108): everyone in the family reads, admins create, change and delete. Max. 20 per family.</summary>
public sealed class GroupsController : ApiControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <summary>GET /api/v1/groups/list</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GroupDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<GroupDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _groupService.ListAsync(organizationId, userId, cancellationToken) is not { } groups)
        {
            return Forbid();
        }

        return groups.Select(GroupDto.From).ToList();
    }

    /// <summary>POST /api/v1/groups/create</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GroupDto>> CreateAsync(CreateGroupRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _groupService.CreateAsync(
            organizationId, userId, new GroupInput(request.Name, request.LearnerIds), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>POST /api/v1/groups/update – renames the group and sets exactly the given children.</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<GroupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupDto>> UpdateAsync(UpdateGroupRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _groupService.UpdateAsync(
            organizationId, userId, request.Id, new GroupInput(request.Name, request.LearnerIds), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>POST /api/v1/groups/delete – the children stay.</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(DeleteGroupRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var status = await _groupService.DeleteAsync(organizationId, userId, request.Id, cancellationToken);
        return status == GroupChangeStatus.Success ? NoContent() : NotFound();
    }

    private ActionResult<GroupDto> ToResponse(GroupChangeResult result) => result.Status switch
    {
        GroupChangeStatus.Success => GroupDto.From(result.Group!),
        GroupChangeStatus.Invalid => FieldErrors(result.Errors!),
        GroupChangeStatus.LimitReached => Problem(statusCode: StatusCodes.Status409Conflict, title: "Too many groups"),
        _ => NotFound(),
    };
}
