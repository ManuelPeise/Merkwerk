using Data.Accessor.Abstractions;
using Data.Database.Entities.Groups;
using Data.Database.Entities.Learners;
using Logic.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models.Organizations;

namespace Logic.Organizations.Groups;

/// <summary>
/// Groups of children (LP-108): every adult of the family reads them, only admins change them. A group only ever holds
/// children of its own family; IDs of other families are reported, never silently dropped.
/// </summary>
internal sealed class GroupService : IGroupService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMemberService _members;

    public GroupService(IUnitOfWorkFactory unitOfWorkFactory, IMemberService members)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _members = members;
    }

    public async Task<IReadOnlyList<GroupInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var groups = await unitOfWork.Repository<GroupEntity>().Query()
            .Where(g => g.OrganizationId == organizationId)
            .Include(g => g.Learners)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

        return groups.Select(ToInfo).ToList();
    }

    public async Task<GroupChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        GroupInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new GroupChangeResult(GroupChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var groups = unitOfWork.Repository<GroupEntity>();

        var count = await groups.Query().CountAsync(g => g.OrganizationId == organizationId, cancellationToken);
        if (count >= GroupRules.MaxPerOrganization)
        {
            return new GroupChangeResult(GroupChangeStatus.LimitReached);
        }

        var errors = await ValidateAsync(unitOfWork, organizationId, input, groupId: null, cancellationToken);
        if (errors.Count > 0)
        {
            return new GroupChangeResult(GroupChangeStatus.Invalid, Errors: errors);
        }

        var group = new GroupEntity { OrganizationId = organizationId, Name = input.Name.Trim() };
        SetLearners(group, organizationId, input.LearnerIds);
        groups.Add(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroupChangeResult(GroupChangeStatus.Success, ToInfo(group));
    }

    public async Task<GroupChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long groupId,
        GroupInput input,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return new GroupChangeResult(GroupChangeStatus.NotFound);
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var group = await unitOfWork.Repository<GroupEntity>().QueryTracked()
            .Include(g => g.Learners)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        // Explicit tenant check in addition to the query filter (ADR 007).
        if (group is null || group.OrganizationId != organizationId)
        {
            return new GroupChangeResult(GroupChangeStatus.NotFound);
        }

        var errors = await ValidateAsync(unitOfWork, organizationId, input, groupId, cancellationToken);
        if (errors.Count > 0)
        {
            return new GroupChangeResult(GroupChangeStatus.Invalid, Errors: errors);
        }

        group.Name = input.Name.Trim();
        SetLearners(group, organizationId, input.LearnerIds);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroupChangeResult(GroupChangeStatus.Success, ToInfo(group));
    }

    public async Task<GroupChangeStatus> DeleteAsync(
        long organizationId,
        long actingUserId,
        long groupId,
        CancellationToken cancellationToken)
    {
        if (!await _members.IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return GroupChangeStatus.NotFound;
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var groups = unitOfWork.Repository<GroupEntity>();
        var group = await groups.GetByIdAsync(groupId, cancellationToken);

        if (group is null || group.OrganizationId != organizationId)
        {
            return GroupChangeStatus.NotFound;
        }

        // The children of the group go with it (cascade in the database).
        groups.Remove(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return GroupChangeStatus.Success;
    }

    /// <summary>Makes the group hold exactly the given children: removes the others, adds the missing ones.</summary>
    private static void SetLearners(GroupEntity group, long organizationId, IReadOnlyCollection<long> learnerIds)
    {
        var wanted = learnerIds.ToHashSet();

        group.Learners.RemoveAll(l => !wanted.Contains(l.LearnerId));

        foreach (var learnerId in wanted.Except(group.Learners.Select(l => l.LearnerId)))
        {
            group.Learners.Add(new GroupLearnerEntity { OrganizationId = organizationId, LearnerId = learnerId });
        }
    }

    private static GroupInfo ToInfo(GroupEntity group) =>
        new(group.Id, group.Name, group.Learners.Select(l => l.LearnerId).Order().ToList());

    private static async Task<Dictionary<string, string[]>> ValidateAsync(
        IUnitOfWork unitOfWork,
        long organizationId,
        GroupInput input,
        long? groupId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var name = input.Name?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > GroupRules.NameMaxLength)
        {
            errors["name"] = [$"Required, at most {GroupRules.NameMaxLength} characters."];
        }
        else if (await unitOfWork.Repository<GroupEntity>().Query()
            .AnyAsync(g => g.OrganizationId == organizationId && g.Name == name && g.Id != groupId, cancellationToken))
        {
            errors["name"] = ["A group with this name already exists."];
        }

        var learnerIds = (input.LearnerIds ?? []).Distinct().ToList();
        if (learnerIds.Count > 0)
        {
            var known = await unitOfWork.Repository<LearnerEntity>().Query()
                .CountAsync(l => l.OrganizationId == organizationId && learnerIds.Contains(l.Id), cancellationToken);

            if (known != learnerIds.Count)
            {
                errors["learnerIds"] = ["Unknown child."];
            }
        }

        return errors;
    }
}
