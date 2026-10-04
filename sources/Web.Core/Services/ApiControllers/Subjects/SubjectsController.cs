using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Subjects;
using Web.Core.Services.ApiControllers.Subjects.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Subjects;

/// <summary>Subjects with color and icon (LP-109): every adult reads, the family's admin creates and changes.</summary>
public sealed class SubjectsController : ApiControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    /// <summary>GET /api/v1/subjects/list</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SubjectDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<SubjectDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _subjectService.ListAsync(organizationId, userId, cancellationToken) is not { } subjects)
        {
            return Forbid();
        }

        return subjects.Select(SubjectDto.From).ToList();
    }

    /// <summary>POST /api/v1/subjects/create</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<SubjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectDto>> CreateAsync(CreateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _subjectService.CreateAsync(organizationId, userId, request.ToInput(), cancellationToken);
        return ToResponse(result);
    }

    /// <summary>POST /api/v1/subjects/update</summary>
    [Authorize(Policy = AuthorizationPolicies.OrgAdmin)]
    [HttpPost]
    [ProducesResponseType<SubjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectDto>> UpdateAsync(UpdateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _subjectService.UpdateAsync(organizationId, userId, request.Id, request.ToInput(), cancellationToken);
        return ToResponse(result);
    }

    private ActionResult<SubjectDto> ToResponse(SubjectChangeResult result) => result.Status switch
    {
        SubjectChangeStatus.Success => SubjectDto.From(result.Subject!),
        SubjectChangeStatus.Invalid => FieldErrors(result.Errors!),
        _ => NotFound(),
    };
}
