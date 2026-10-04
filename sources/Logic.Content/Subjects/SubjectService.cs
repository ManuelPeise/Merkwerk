using Data.Accessor.Abstractions;
using Data.Database.Entities.Subjects;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models.Subjects;

namespace Logic.Content.Subjects;

/// <summary>
/// Subjects are instance-wide (shared exercises, LP-201, must mean the same subject everywhere). Every adult reads them,
/// the family's admin creates and changes them (LP-109; with schools this moves to the instance admin, LP-203).
/// </summary>
internal sealed class SubjectService : ISubjectService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMemberService _members;

    public SubjectService(IUnitOfWorkFactory unitOfWorkFactory, IMemberService members)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _members = members;
    }

    public async Task<IReadOnlyList<SubjectInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        return await unitOfWork.Repository<SubjectEntity>().Query()
            .OrderBy(s => s.Name)
            .Select(s => new SubjectInfo(s.Id, s.Name, s.LanguageCode, s.Color, s.Icon))
            .ToListAsync(cancellationToken);
    }

    public async Task<SubjectChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        SubjectInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new SubjectChangeResult(SubjectChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var subjects = unitOfWork.Repository<SubjectEntity>();

        var errors = await ValidateAsync(subjects, input, subjectId: null, cancellationToken);
        if (errors.Count > 0)
        {
            return new SubjectChangeResult(SubjectChangeStatus.Invalid, Errors: errors);
        }

        var subject = new SubjectEntity();
        Apply(subject, input);
        subjects.Add(subject);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubjectChangeResult(SubjectChangeStatus.Success, ToInfo(subject));
    }

    public async Task<SubjectChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long subjectId,
        SubjectInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new SubjectChangeResult(SubjectChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var subjects = unitOfWork.Repository<SubjectEntity>();
        var subject = await subjects.GetByIdAsync(subjectId, cancellationToken);

        if (subject is null)
        {
            return new SubjectChangeResult(SubjectChangeStatus.NotFound);
        }

        var errors = await ValidateAsync(subjects, input, subjectId, cancellationToken);
        if (errors.Count > 0)
        {
            return new SubjectChangeResult(SubjectChangeStatus.Invalid, Errors: errors);
        }

        Apply(subject, input);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new SubjectChangeResult(SubjectChangeStatus.Success, ToInfo(subject));
    }

    private static void Apply(SubjectEntity subject, SubjectInput input)
    {
        subject.Name = input.Name.Trim();
        subject.LanguageCode = input.LanguageCode;
        subject.Color = input.Color;
        subject.Icon = input.Icon;
    }

    private static SubjectInfo ToInfo(SubjectEntity subject) =>
        new(subject.Id, subject.Name, subject.LanguageCode, subject.Color, subject.Icon);

    private static async Task<Dictionary<string, string[]>> ValidateAsync(
        IRepository<SubjectEntity> subjects,
        SubjectInput input,
        long? subjectId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var name = input.Name?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > SubjectRules.NameMaxLength)
        {
            errors["name"] = [$"Required, at most {SubjectRules.NameMaxLength} characters."];
        }
        else if (await subjects.Query().AnyAsync(s => s.Name == name && s.Id != subjectId, cancellationToken))
        {
            // The column collation ignores case and accents, like the unique index.
            errors["name"] = ["A subject with this name already exists."];
        }

        if (!SubjectRules.LanguageCodes.Contains(input.LanguageCode))
        {
            errors["languageCode"] = ["Unknown language."];
        }

        if (!SubjectRules.Colors.Contains(input.Color))
        {
            errors["color"] = ["Unknown color."];
        }

        if (!SubjectRules.Icons.Contains(input.Icon))
        {
            errors["icon"] = ["Unknown icon."];
        }

        return errors;
    }
}
