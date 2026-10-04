using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models.Exercises;
using Web.Core.Services.ApiControllers.Exercises.Dtos;
using Web.Core.Services.Authorization;

namespace Web.Core.Services.ApiControllers.Exercises;

/// <summary>
/// Exercises of the family (LP-110): every adult creates, edits, publishes and archives them. Question payloads and
/// solutions travel as polymorphic JSON with a <c>type</c> discriminator (ADR 005). The editor UI follows in LP-112.
/// </summary>
public sealed class ExercisesController : ApiControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    /// <summary>GET /api/v1/exercises/list – archived ones included (see state).</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ExerciseSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ExerciseSummaryDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _exerciseService.ListAsync(organizationId, userId, cancellationToken) is not { } exercises)
        {
            return Forbid();
        }

        return exercises.Select(ExerciseSummaryDto.From).ToList();
    }

    /// <summary>GET /api/v1/exercises/get?id=… – the draft with its questions.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<ExerciseDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseDetailDto>> GetAsync([FromQuery] long id, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _exerciseService.GetAsync(organizationId, userId, id, cancellationToken) is not { } exercise)
        {
            return NotFound();
        }

        return ExerciseDetailDto.From(exercise);
    }

    /// <summary>GET /api/v1/exercises/version?id=…&amp;number=… – a published, frozen version.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<ExerciseVersionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseVersionDto>> VersionAsync(
        [FromQuery] long id,
        [FromQuery] int number,
        CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await _exerciseService.GetVersionAsync(organizationId, userId, id, number, cancellationToken) is not { } version)
        {
            return NotFound();
        }

        return ExerciseVersionDto.From(id, number, version);
    }

    /// <summary>POST /api/v1/exercises/create – a new draft.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<ExerciseSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExerciseSummaryDto>> CreateAsync(SaveExerciseRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        return ToResponse(await _exerciseService.CreateAsync(organizationId, userId, request.ToInput(), cancellationToken));
    }

    /// <summary>POST /api/v1/exercises/update – saves the whole draft; questions are replaced in the given order.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<ExerciseSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseSummaryDto>> UpdateAsync(UpdateExerciseRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _exerciseService.UpdateAsync(organizationId, userId, request.Id, request.Exercise.ToInput(), cancellationToken);
        return ToResponse(result);
    }

    /// <summary>POST /api/v1/exercises/publish – freezes the draft as the next version.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<ExerciseSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseSummaryDto>> PublishAsync(ExerciseIdRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        return ToResponse(await _exerciseService.PublishAsync(organizationId, userId, request.Id, cancellationToken));
    }

    /// <summary>POST /api/v1/exercises/archive – archives or restores; versions stay valid.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<ExerciseSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseSummaryDto>> ArchiveAsync(ArchiveExerciseRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return Forbid();
        }

        var result = await _exerciseService.SetArchivedAsync(organizationId, userId, request.Id, request.Archived, cancellationToken);
        return ToResponse(result);
    }

    private ActionResult<ExerciseSummaryDto> ToResponse(ExerciseChangeResult result) => result.Status switch
    {
        ExerciseChangeStatus.Success => ExerciseSummaryDto.From(result.Exercise!),
        ExerciseChangeStatus.Invalid => FieldErrors(result.Errors!),
        _ => NotFound(),
    };
}
