using Shared.Enums;
using Shared.Models.Assignments;

namespace Logic.Shared.Interfaces;

/// <summary>
/// Assigning exercises to children and groups (LP-114). Every adult of the family assigns and revokes. Only published,
/// not archived exercises can be assigned; children always get the latest published version.
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Assignments of one exercise, children first, then groups; null if the exercise is unknown here or the acting user
    /// is no member.
    /// </summary>
    Task<IReadOnlyList<AssignmentInfo>?> ListForExerciseAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        CancellationToken cancellationToken);

    /// <summary>Assigns to every given child and group; existing assignments of the same exercise take the new options.</summary>
    Task<AssignmentChangeResult> AssignAsync(
        long organizationId,
        long actingUserId,
        AssignmentInput input,
        CancellationToken cancellationToken);

    /// <summary>Takes the assignment back.</summary>
    Task<AssignmentChangeStatus> RevokeAsync(
        long organizationId,
        long actingUserId,
        long assignmentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// What the child has to do: its own assignments and those of its groups (dynamic), one per exercise, due ones first.
    /// A direct assignment wins over a group's; among groups the earliest due date wins. Archived exercises are left out.
    /// For the children's API (LP-115) – the caller has checked the child's session.
    /// </summary>
    Task<IReadOnlyList<LearnerAssignment>> ListForLearnerAsync(
        long organizationId,
        long learnerId,
        CancellationToken cancellationToken);
}
