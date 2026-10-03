using Shared.Enums;
using Shared.Models.Organizations;

namespace Logic.Shared.Interfaces;

/// <summary>Child profiles of a family (LP-105). Everyone in the family reads, only admins change.</summary>
public interface ILearnerService
{
    /// <summary>Null if the acting user is no member of the organization (any more).</summary>
    Task<IReadOnlyList<LearnerInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    Task<LearnerChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        LearnerInput input,
        CancellationToken cancellationToken);

    Task<LearnerChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long learnerId,
        LearnerInput input,
        CancellationToken cancellationToken);

    Task<LearnerChangeStatus> DeleteAsync(
        long organizationId,
        long actingUserId,
        long learnerId,
        CancellationToken cancellationToken);
}
