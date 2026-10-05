using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Web.Core.Services.ApiControllers.Assignments.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Assignments;

/// <summary>
/// Assigning exercises to children and groups (LP-114): every adult of the family assigns and revokes. A group
/// assignment reaches whoever is in the group. The children's side follows with LP-115.
/// </summary>
public sealed class AssignmentsController : ApiControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>GET /api/v1/assignments/list?exerciseId=… – children first, then groups.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> ListAsync(
        [FromQuery] long exerciseId,
        CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _assignmentService.ListForExerciseAsync(organizationId, userId, exerciseId, cancellationToken) is not { } assignments)
        {
            return NotFound();
        }

        return assignments.Select(AssignmentDto.From).ToList();
    }

    /// <summary>POST /api/v1/assignments/assign – returns the assignments created or changed.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> AssignAsync(
        AssignExerciseRequestDto request,
        CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _assignmentService.AssignAsync(organizationId, userId, request.ToInput(), cancellationToken);

        return result.Status switch
        {
            AssignmentChangeStatus.Success => result.Assignments!.Select(AssignmentDto.From).ToList(),
            AssignmentChangeStatus.Invalid => FieldErrors(result.Errors!),
            _ => NotFound(),
        };
    }

    /// <summary>POST /api/v1/assignments/revoke – takes the assignment back.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(RevokeAssignmentRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var status = await _assignmentService.RevokeAsync(organizationId, userId, request.Id, cancellationToken);
        return status == AssignmentChangeStatus.Success ? NoContent() : NotFound();
    }
}
