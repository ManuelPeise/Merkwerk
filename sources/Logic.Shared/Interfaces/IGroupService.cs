using Shared.Enums;
using Shared.Models.Organizations;

namespace Logic.Shared.Interfaces;

/// <summary>Groups of children for assigning exercises together (LP-108). Everyone in the family reads, only admins change.</summary>
public interface IGroupService
{
    /// <summary>Groups ordered by name; null if the acting user is no member of the organization (any more).</summary>
    Task<IReadOnlyList<GroupInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    /// <summary>Creates a group with its children (may be empty).</summary>
    Task<GroupChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        GroupInput input,
        CancellationToken cancellationToken);

    /// <summary>Renames the group and sets exactly the given children.</summary>
    Task<GroupChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long groupId,
        GroupInput input,
        CancellationToken cancellationToken);

    /// <summary>Deletes the group; the children stay.</summary>
    Task<GroupChangeStatus> DeleteAsync(
        long organizationId,
        long actingUserId,
        long groupId,
        CancellationToken cancellationToken);
}
