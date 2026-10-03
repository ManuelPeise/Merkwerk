using Data.Accessor.Abstractions;
using Data.Database.Entities.Learners;
using Logic.Organizations.Members;
using Microsoft.EntityFrameworkCore;

namespace Logic.Organizations.Learners;

internal sealed class LearnerService(IUnitOfWorkFactory unitOfWorkFactory, IMemberService members) : ILearnerService
{
    public async Task<IReadOnlyList<LearnerInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        if (!await members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        return await unitOfWork.Repository<Learner>().Query()
            .Where(l => l.OrganizationId == organizationId)
            .OrderBy(l => l.DisplayName)
            .Select(l => new LearnerInfo(l.Id, l.DisplayName, l.Grade, l.AvatarId))
            .ToListAsync(cancellationToken);
    }

    public async Task<LearnerChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        LearnerInput input,
        CancellationToken cancellationToken)
    {
        if (!await members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new LearnerChangeResult(LearnerChangeStatus.NotFound);
        }

        var errors = Validate(input);
        if (errors.Count > 0)
        {
            return new LearnerChangeResult(LearnerChangeStatus.Invalid, Errors: errors);
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        var learners = unitOfWork.Repository<Learner>();
        var count = await learners.Query().CountAsync(l => l.OrganizationId == organizationId, cancellationToken);

        if (count >= LearnerRules.MaxPerOrganization)
        {
            return new LearnerChangeResult(LearnerChangeStatus.LimitReached);
        }

        var learner = new Learner { OrganizationId = organizationId };
        Apply(learner, input);
        learners.Add(learner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LearnerChangeResult(LearnerChangeStatus.Success, ToInfo(learner));
    }

    public async Task<LearnerChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long learnerId,
        LearnerInput input,
        CancellationToken cancellationToken)
    {
        if (!await members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new LearnerChangeResult(LearnerChangeStatus.NotFound);
        }

        var errors = Validate(input);
        if (errors.Count > 0)
        {
            return new LearnerChangeResult(LearnerChangeStatus.Invalid, Errors: errors);
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        var learner = await unitOfWork.Repository<Learner>().GetByIdAsync(learnerId, cancellationToken);

        // Explicit tenant check in addition to the query filter (ADR 007).
        if (learner is null || learner.OrganizationId != organizationId)
        {
            return new LearnerChangeResult(LearnerChangeStatus.NotFound);
        }

        Apply(learner, input);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new LearnerChangeResult(LearnerChangeStatus.Success, ToInfo(learner));
    }

    public async Task<LearnerChangeStatus> DeleteAsync(
        long organizationId,
        long actingUserId,
        long learnerId,
        CancellationToken cancellationToken)
    {
        if (!await members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return LearnerChangeStatus.NotFound;
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        var learners = unitOfWork.Repository<Learner>();
        var learner = await learners.GetByIdAsync(learnerId, cancellationToken);

        if (learner is null || learner.OrganizationId != organizationId)
        {
            return LearnerChangeStatus.NotFound;
        }

        learners.Remove(learner);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return LearnerChangeStatus.Success;
    }

    private static void Apply(Learner learner, LearnerInput input)
    {
        learner.DisplayName = input.DisplayName.Trim();
        learner.Grade = input.Grade;
        learner.AvatarId = input.AvatarId;
    }

    private static LearnerInfo ToInfo(Learner learner) =>
        new(learner.Id, learner.DisplayName, learner.Grade, learner.AvatarId);

    private static Dictionary<string, string[]> Validate(LearnerInput input)
    {
        var errors = new Dictionary<string, string[]>();
        var name = input.DisplayName?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > Learner.DisplayNameMaxLength)
        {
            errors["displayName"] = [$"Required, at most {Learner.DisplayNameMaxLength} characters."];
        }

        if (input.Grade is < LearnerRules.MinGrade or > LearnerRules.MaxGrade)
        {
            errors["grade"] = [$"Between {LearnerRules.MinGrade} and {LearnerRules.MaxGrade}."];
        }

        if (!LearnerRules.AvatarIds.Contains(input.AvatarId))
        {
            errors["avatarId"] = ["Unknown avatar."];
        }

        return errors;
    }
}
