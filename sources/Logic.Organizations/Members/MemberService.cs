using Data.Accessor.Abstractions;
using Data.Database.Entities.Organizations;
using Logic.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Logic.Organizations.Members;

internal sealed class MemberService(IUnitOfWorkFactory unitOfWorkFactory, IAccountService accounts) : IMemberService
{
    public async Task<IReadOnlyList<MemberInfo>?> ListAsync(
        long organizationId,
        long actingUserId,
        CancellationToken cancellationToken)
    {
        // Checked against the database: a removed member keeps the organization claim for up to 15 minutes.
        if (!await IsMemberAsync(organizationId, actingUserId, cancellationToken))
        {
            return null;
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        var memberships = await unitOfWork.Memberships.Query()
            .Where(m => m.OrganizationId == organizationId)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        var accountsById = (await accounts.GetAccountsAsync(memberships.Select(m => m.UserId).ToList(), cancellationToken))
            .ToDictionary(a => a.UserId);

        return memberships
            .Where(m => accountsById.ContainsKey(m.UserId))
            .Select(m => new MemberInfo(
                m.Id, m.UserId, accountsById[m.UserId].DisplayName, accountsById[m.UserId].Email, m.Role, m.IsOwner))
            .ToList();
    }

    public async Task<RemoveMemberStatus> RemoveAsync(
        long organizationId,
        long actingUserId,
        long membershipId,
        CancellationToken cancellationToken)
    {
        if (!await IsAdminAsync(organizationId, actingUserId, cancellationToken))
        {
            return RemoveMemberStatus.NotFound;
        }

        await using var unitOfWork = unitOfWorkFactory.Create();
        var membership = await unitOfWork.Memberships.GetByIdAsync(membershipId, cancellationToken);

        if (membership is null || membership.OrganizationId != organizationId)
        {
            return RemoveMemberStatus.NotFound;
        }

        if (membership.IsOwner)
        {
            return RemoveMemberStatus.IsOwner;
        }

        if (membership.UserId == actingUserId)
        {
            return RemoveMemberStatus.IsSelf;
        }

        unitOfWork.Memberships.Remove(membership);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return RemoveMemberStatus.Success;
    }

    public async Task<bool> IssueStartPasswordAsync(
        long organizationId,
        long actingUserId,
        long targetUserId,
        string language,
        CancellationToken cancellationToken)
    {
        if (!await IsAdminAsync(organizationId, actingUserId, cancellationToken)
            || !await IsMemberAsync(organizationId, targetUserId, cancellationToken))
        {
            return false;
        }

        return (await accounts.IssueStartPasswordAsync(targetUserId, language, cancellationToken)).Succeeded;
    }

    public async Task<bool> IsMemberAsync(long organizationId, long userId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        return await unitOfWork.Memberships.FindAsync(organizationId, userId, cancellationToken) is not null;
    }

    public async Task<bool> IsAdminAsync(long organizationId, long userId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var membership = await unitOfWork.Memberships.FindAsync(organizationId, userId, cancellationToken);
        return membership?.Role == OrganizationRole.OrgAdmin;
    }
}
