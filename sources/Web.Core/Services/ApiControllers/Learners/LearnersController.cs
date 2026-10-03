using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Organizations;
using Web.Core.Services.ApiControllers.Learners.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Learners;

/// <summary>Child profiles of the family (LP-105): everyone reads, admins change. Max. 10 per family.</summary>
public sealed class LearnersController(ILearnerService learnerService) : ApiControllerBase
{
    /// <summary>GET /api/v1/learners/list</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LearnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<LearnerDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await learnerService.ListAsync(organizationId, userId, cancellationToken) is not { } learners)
        {
            return Forbid();
        }

        return learners.Select(LearnerDto.From).ToList();
    }

    /// <summary>POST /api/v1/learners/create</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<LearnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LearnerDto>> CreateAsync(CreateLearnerRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await learnerService.CreateAsync(
            organizationId, userId, new LearnerInput(request.DisplayName, request.Grade, request.AvatarId), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>POST /api/v1/learners/update</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<LearnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LearnerDto>> UpdateAsync(UpdateLearnerRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await learnerService.UpdateAsync(
            organizationId,
            userId,
            request.Id,
            new LearnerInput(request.DisplayName, request.Grade, request.AvatarId),
            cancellationToken);

        return ToResponse(result);
    }

    /// <summary>POST /api/v1/learners/delete</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(DeleteLearnerRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var status = await learnerService.DeleteAsync(organizationId, userId, request.Id, cancellationToken);
        return status == LearnerChangeStatus.Success ? NoContent() : NotFound();
    }

    private ActionResult<LearnerDto> ToResponse(LearnerChangeResult result) => result.Status switch
    {
        LearnerChangeStatus.Success => LearnerDto.From(result.Learner!),
        LearnerChangeStatus.Invalid => FieldErrors(result.Errors!),
        LearnerChangeStatus.LimitReached => Problem(statusCode: StatusCodes.Status409Conflict, title: "Too many children"),
        _ => NotFound(),
    };
}
