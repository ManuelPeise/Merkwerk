using Data.Accessor.Abstractions;
using Data.Database.Entities.Exercises;
using Data.Database.Entities.Subjects;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models.Exercises;

namespace Logic.Content.Exercises;

/// <summary>
/// Exercises of a family (LP-110): every adult creates, edits, publishes and archives them. Publishing freezes the
/// draft as a numbered version (ADR 005); the draft stays editable and later publishing creates the next version.
/// Generator exercises (LP-131) store settings instead of questions.
/// </summary>
internal sealed class ExerciseService : IExerciseService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMemberService _members;
    private readonly IGeneratorService _generators;
    private readonly TimeProvider _timeProvider;

    public ExerciseService(
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

    public async Task<IReadOnlyList<ExerciseSummary>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var exercises = await unitOfWork.Repository<ExerciseEntity>().Query()
            .Where(e => e.OrganizationId == organizationId)
            .Include(e => e.Questions)
            .OrderBy(e => e.Title)
            .ToListAsync(cancellationToken);

        return exercises.Select(ToSummary).ToList();
    }

    public async Task<ExerciseDetail?> GetAsync(
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
        var exercise = await LoadAsync(unitOfWork.Repository<ExerciseEntity>().Query(), organizationId, exerciseId, cancellationToken);

        return exercise is null ? null : new ExerciseDetail(ToSummary(exercise), ToContent(exercise), exercise.Generator);
    }

    public async Task<ExerciseChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        ExerciseInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();

        var errors = await ValidateAsync(unitOfWork, input, _generators, cancellationToken);
        if (errors.Count > 0)
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.Invalid, Errors: errors);
        }

        var exercise = new ExerciseEntity { OrganizationId = organizationId };
        Apply(exercise, input);
        unitOfWork.Repository<ExerciseEntity>().Add(exercise);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExerciseChangeResult(ExerciseChangeStatus.Success, ToSummary(exercise));
    }

    public async Task<ExerciseChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        ExerciseInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var exercise = await LoadAsync(unitOfWork.Repository<ExerciseEntity>().QueryTracked(), organizationId, exerciseId, cancellationToken);

        if (exercise is null)
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        var errors = await ValidateAsync(unitOfWork, input, _generators, cancellationToken);
        if (errors.Count > 0)
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.Invalid, Errors: errors);
        }

        Apply(exercise, input);
        exercise.HasUnpublishedChanges = exercise.LatestVersion > 0;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExerciseChangeResult(ExerciseChangeStatus.Success, ToSummary(exercise));
    }

    public async Task<ExerciseChangeResult> PublishAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var exercise = await LoadAsync(unitOfWork.Repository<ExerciseEntity>().QueryTracked(), organizationId, exerciseId, cancellationToken);

        if (exercise is null)
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        if (exercise.ContentSource == ExerciseContentSource.Questions && exercise.Questions.Count == 0)
        {
            return new ExerciseChangeResult(
                ExerciseChangeStatus.Invalid,
                Errors: new Dictionary<string, string[]> { ["questions"] = ["Add at least one question before publishing."] });
        }

        if (exercise.LatestVersion > 0 && !exercise.HasUnpublishedChanges)
        {
            // Nothing new – publishing again would only create an identical version.
            return new ExerciseChangeResult(ExerciseChangeStatus.Success, ToSummary(exercise));
        }

        exercise.LatestVersion++;
        exercise.HasUnpublishedChanges = false;
        unitOfWork.Repository<ExerciseVersionEntity>().Add(new ExerciseVersionEntity
        {
            OrganizationId = organizationId,
            ExerciseId = exercise.Id,
            Number = exercise.LatestVersion,
            PublishedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Content = new ExerciseSnapshot(
                exercise.Title, exercise.SubjectId, exercise.Grade, exercise.ContentSource, ToContent(exercise), exercise.Generator),
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ExerciseChangeResult(ExerciseChangeStatus.Success, ToSummary(exercise));
    }

    public async Task<ExerciseChangeResult> SetArchivedAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        bool archived,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var exercise = await LoadAsync(unitOfWork.Repository<ExerciseEntity>().QueryTracked(), organizationId, exerciseId, cancellationToken);

        if (exercise is null)
        {
            return new ExerciseChangeResult(ExerciseChangeStatus.NotFound);
        }

        exercise.IsArchived = archived;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ExerciseChangeResult(ExerciseChangeStatus.Success, ToSummary(exercise));
    }

    public async Task<ExerciseSnapshot?> GetVersionAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        int versionNumber,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        return await unitOfWork.Repository<ExerciseVersionEntity>().Query()
            .Where(v => v.OrganizationId == organizationId && v.ExerciseId == exerciseId && v.Number == versionNumber)
            .Select(v => v.Content)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Exercise with its questions; null if unknown or of another family (explicit check besides the query filter, ADR 007).</summary>
    private static async Task<ExerciseEntity?> LoadAsync(
        IQueryable<ExerciseEntity> exercises,
        long organizationId,
        long exerciseId,
        CancellationToken cancellationToken)
    {
        var exercise = await exercises.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);
        return exercise is null || exercise.OrganizationId != organizationId ? null : exercise;
    }

    /// <summary>Takes over the input; the questions are replaced as a whole, in the given order.</summary>
    private static void Apply(ExerciseEntity exercise, ExerciseInput input)
    {
        exercise.Title = input.Title.Trim();
        exercise.SubjectId = input.SubjectId;
        exercise.Grade = input.Grade;
        exercise.ContentSource = input.ContentSource;
        exercise.Generator = input.ContentSource == ExerciseContentSource.Generator ? input.Generator : null;

        exercise.Questions.Clear();
        exercise.Questions.AddRange((input.Questions ?? []).Select((question, index) => new QuestionEntity
        {
            OrganizationId = exercise.OrganizationId,
            Position = index,
            Payload = question.Payload,
            Solution = question.Solution,
        }));
    }

    private static IReadOnlyList<QuestionContent> ToContent(ExerciseEntity exercise) =>
        exercise.Questions.OrderBy(q => q.Position).Select(q => new QuestionContent(q.Payload, q.Solution)).ToList();

    private static ExerciseSummary ToSummary(ExerciseEntity exercise) => new(
        exercise.Id,
        exercise.Title,
        exercise.SubjectId,
        exercise.Grade,
        exercise.ContentSource,
        exercise.IsArchived ? ExerciseState.Archived : exercise.LatestVersion > 0 ? ExerciseState.Published : ExerciseState.Draft,
        exercise.LatestVersion,
        exercise.HasUnpublishedChanges,
        exercise.Generator?.TaskCount ?? exercise.Questions.Count);

    private static async Task<Dictionary<string, string[]>> ValidateAsync(
        IUnitOfWork unitOfWork,
        ExerciseInput input,
        IGeneratorService generators,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var title = input.Title?.Trim() ?? string.Empty;

        if (title.Length == 0 || title.Length > ExerciseRules.TitleMaxLength)
        {
            errors["title"] = [$"Required, at most {ExerciseRules.TitleMaxLength} characters."];
        }

        if (input.Grade is < ExerciseRules.MinGrade or > ExerciseRules.MaxGrade)
        {
            errors["grade"] = [$"Between {ExerciseRules.MinGrade} and {ExerciseRules.MaxGrade}."];
        }

        if (!await unitOfWork.Repository<SubjectEntity>().Query().AnyAsync(s => s.Id == input.SubjectId, cancellationToken))
        {
            errors["subjectId"] = ["Unknown subject."];
        }

        var questions = input.Questions ?? [];

        switch (input.ContentSource)
        {
            case ExerciseContentSource.Questions:
                if (input.Generator is not null)
                {
                    errors["generator"] = ["Only generator exercises have generator settings."];
                }

                break;
            case ExerciseContentSource.Generator:
                foreach (var (field, messages) in generators.Validate(input.Generator))
                {
                    errors[field] = messages;
                }

                if (questions.Count > 0)
                {
                    errors["questions"] = ["Generator exercises have no questions of their own."];
                }

                break;
            default:
                // Word lists (LP-140) are not supported yet.
                errors["contentSource"] = ["Only exercises with questions or a generator are supported so far."];
                break;
        }
        if (questions.Count > ExerciseRules.MaxQuestions)
        {
            errors["questions"] = [$"At most {ExerciseRules.MaxQuestions} questions."];
        }

        for (var index = 0; index < questions.Count; index++)
        {
            var problems = QuestionValidator.Validate(questions[index]);
            if (problems.Count > 0)
            {
                errors[$"questions[{index}]"] = problems.ToArray();
            }
        }

        return errors;
    }
}
