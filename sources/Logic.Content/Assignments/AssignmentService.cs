using Data.Accessor.Abstractions;
using Data.Database.Entities.Assignments;
using Data.Database.Entities.Exercises;
using Data.Database.Entities.Groups;
using Data.Database.Entities.Learners;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models.Assignments;

namespace Logic.Content.Assignments;

/// <summary>
/// Assigning exercises to children and groups (LP-114): every adult of the family assigns and revokes. A group
/// assignment is stored once and reaches whoever is in the group (dynamic). Children always get the latest published
/// version; archived exercises disappear from their list but keep their assignments.
/// </summary>
internal sealed class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMemberService _members;
    private readonly IGeneratorService _generators;
    private readonly TimeProvider _timeProvider;

    public AssignmentService(
        IUnitOfWorkFactory unitOfWorkFactory,
        IMemberService members,
        IGeneratorService generators,
        TimeProvider timeProvider)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _members = members;
        _generators = generators;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<AssignmentInfo>?> ListForExerciseAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        if (await FindExerciseAsync(unitOfWork, organizationId, exerciseId, cancellationToken) is null)
        {
            return null;
        }

        var assignments = await unitOfWork.Repository<AssignmentEntity>().Query()
            .Where(a => a.OrganizationId == organizationId && a.ExerciseId == exerciseId)
            .OrderBy(a => a.GroupId != null)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

        return assignments.Select(ToInfo).ToList();
    }

    public async Task<AssignmentChangeResult> AssignAsync(
        long organizationId,
        long actingUserId,
        AssignmentInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return new AssignmentChangeResult(AssignmentChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var exercise = await FindExerciseAsync(unitOfWork, organizationId, input.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            return new AssignmentChangeResult(AssignmentChangeStatus.NotFound);
        }

        var learnerIds = (input.LearnerIds ?? []).Distinct().ToList();
        var groupIds = (input.GroupIds ?? []).Distinct().ToList();

        var errors = await ValidateAsync(unitOfWork, organizationId, exercise, input, learnerIds, groupIds, cancellationToken);
        if (errors.Count > 0)
        {
            return new AssignmentChangeResult(AssignmentChangeStatus.Invalid, Errors: errors);
        }

        var exerciseId = exercise.Id;
        var repository = unitOfWork.Repository<AssignmentEntity>();
        var existing = await repository.QueryTracked()
            .Where(a => a.OrganizationId == organizationId && a.ExerciseId == exerciseId
                && ((a.LearnerId != null && learnerIds.Contains(a.LearnerId.Value))
                    || (a.GroupId != null && groupIds.Contains(a.GroupId.Value))))
            .ToListAsync(cancellationToken);

        var changed = new List<AssignmentEntity>();

        foreach (var learnerId in learnerIds)
        {
            changed.Add(Upsert(existing.FirstOrDefault(a => a.LearnerId == learnerId), a => a.LearnerId = learnerId));
        }

        foreach (var groupId in groupIds)
        {
            changed.Add(Upsert(existing.FirstOrDefault(a => a.GroupId == groupId), a => a.GroupId = groupId));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AssignmentChangeResult(AssignmentChangeStatus.Success, changed.Select(ToInfo).ToList());

        AssignmentEntity Upsert(AssignmentEntity? assignment, Action<AssignmentEntity> setTarget)
        {
            if (assignment is null)
            {
                assignment = new AssignmentEntity { OrganizationId = organizationId, ExerciseId = exerciseId };
                setTarget(assignment);
                repository.Add(assignment);
            }

            assignment.DueDate = input.DueDate;
            assignment.AllowDotArray = input.AllowDotArray;
            assignment.AllowTimesTableMatrix = input.AllowTimesTableMatrix;
            assignment.Generator = input.Generator;
            return assignment;
        }
    }

    public async Task<AssignmentChangeStatus> RevokeAsync(
        long organizationId,
        long actingUserId,
        long assignmentId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return AssignmentChangeStatus.NotFound;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var repository = unitOfWork.Repository<AssignmentEntity>();
        var assignment = await repository.GetByIdAsync(assignmentId, cancellationToken);

        // Explicit tenant check in addition to the query filter (ADR 007).
        if (assignment is null || assignment.OrganizationId != organizationId)
        {
            return AssignmentChangeStatus.NotFound;
        }

        repository.Remove(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AssignmentChangeStatus.Success;
    }

    public async Task<IReadOnlyList<LearnerAssignment>> ListForLearnerAsync(
        long organizationId,
        long learnerId,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();

        if (!await unitOfWork.Repository<LearnerEntity>().Query()
            .AnyAsync(l => l.Id == learnerId && l.OrganizationId == organizationId, cancellationToken))
        {
            return [];
        }

        var groupIds = await unitOfWork.Repository<GroupLearnerEntity>().Query()
            .Where(g => g.OrganizationId == organizationId && g.LearnerId == learnerId)
            .Select(g => g.GroupId)
            .ToListAsync(cancellationToken);

        var candidates = await unitOfWork.Repository<AssignmentEntity>().Query()
            .Where(a => a.OrganizationId == organizationId
                && (a.LearnerId == learnerId || (a.GroupId != null && groupIds.Contains(a.GroupId.Value))))
            .Join(
                unitOfWork.Repository<ExerciseEntity>().Query()
                    .Where(e => e.OrganizationId == organizationId && !e.IsArchived && e.LatestVersion > 0),
                a => a.ExerciseId,
                e => e.Id,
                (a, e) => new { Assignment = a, ExerciseGenerator = e.Generator })
            .ToListAsync(cancellationToken);

        return candidates
            .GroupBy(c => c.Assignment.ExerciseId)
            .Select(perExercise => perExercise
                .OrderBy(c => c.Assignment.LearnerId == null)
                .ThenBy(c => c.Assignment.DueDate ?? DateOnly.MaxValue)
                .ThenBy(c => c.Assignment.Id)
                .First())
            .Select(c => new LearnerAssignment(
                c.Assignment.Id,
                c.Assignment.ExerciseId,
                c.Assignment.DueDate,
                c.Assignment.AllowDotArray,
                c.Assignment.AllowTimesTableMatrix,
                c.Assignment.Generator ?? c.ExerciseGenerator))
            .OrderBy(a => a.DueDate ?? DateOnly.MaxValue)
            .ThenBy(a => a.AssignmentId)
            .ToList();
    }

    /// <summary>The exercise; null if unknown or of another family (explicit check besides the query filter, ADR 007).</summary>
    private static async Task<ExerciseEntity?> FindExerciseAsync(
        IUnitOfWork unitOfWork,
        long organizationId,
        long exerciseId,
        CancellationToken cancellationToken)
    {
        var exercise = await unitOfWork.Repository<ExerciseEntity>().Query()
            .FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);

        return exercise is null || exercise.OrganizationId != organizationId ? null : exercise;
    }

    private static AssignmentInfo ToInfo(AssignmentEntity assignment) => new(
        assignment.Id,
        assignment.ExerciseId,
        assignment.LearnerId,
        assignment.GroupId,
        assignment.DueDate,
        assignment.AllowDotArray,
        assignment.AllowTimesTableMatrix,
        assignment.Generator,
        assignment.CreatedAt);

    private async Task<Dictionary<string, string[]>> ValidateAsync(
        IUnitOfWork unitOfWork,
        long organizationId,
        ExerciseEntity exercise,
        AssignmentInput input,
        IReadOnlyList<long> learnerIds,
        IReadOnlyList<long> groupIds,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (exercise.LatestVersion == 0 || exercise.IsArchived)
        {
            errors["exerciseId"] = ["Only published exercises that are not archived can be assigned."];
        }

        if (learnerIds.Count + groupIds.Count == 0)
        {
            errors["learnerIds"] = ["Choose at least one child or group."];
        }
        else if (learnerIds.Count + groupIds.Count > AssignmentRules.MaxTargets)
        {
            errors["learnerIds"] = [$"At most {AssignmentRules.MaxTargets} children and groups at once."];
        }

        if (learnerIds.Count > 0)
        {
            var known = await unitOfWork.Repository<LearnerEntity>().Query()
                .CountAsync(l => l.OrganizationId == organizationId && learnerIds.Contains(l.Id), cancellationToken);

            if (known != learnerIds.Count)
            {
                errors["learnerIds"] = ["Unknown child."];
            }
        }

        if (groupIds.Count > 0)
        {
            var known = await unitOfWork.Repository<GroupEntity>().Query()
                .CountAsync(g => g.OrganizationId == organizationId && groupIds.Contains(g.Id), cancellationToken);

            if (known != groupIds.Count)
            {
                errors["groupIds"] = ["Unknown group."];
            }
        }

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        if (input.DueDate is { } dueDate && dueDate < today.AddDays(-AssignmentRules.DueDateToleranceDays))
        {
            errors["dueDate"] = ["The due date lies in the past."];
        }

        if (input.Generator is not null)
        {
            if (exercise.ContentSource != ExerciseContentSource.Generator || exercise.Generator is null)
            {
                errors["generator"] = ["Only generator exercises can get their own settings."];
            }
            else if (input.Generator.GetType() != exercise.Generator.GetType())
            {
                errors["generator"] = ["The settings must belong to the exercise's generator."];
            }
            else
            {
                foreach (var (field, messages) in _generators.Validate(input.Generator))
                {
                    errors[field] = messages;
                }
            }
        }

        return errors;
    }
}
