using Shared.Models.Subjects;

namespace Logic.Shared.Interfaces;

/// <summary>
/// Subjects with color and icon (LP-109). Instance-wide; every adult of a family reads them, the family's admin creates
/// and changes them. Deleting comes with exercises (LP-110), which refer to subjects.
/// </summary>
public interface ISubjectService
{
    /// <summary>All subjects, ordered by name; null if the acting user is no member of the organization (any more).</summary>
    Task<IReadOnlyList<SubjectInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    Task<SubjectChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        SubjectInput input,
        CancellationToken cancellationToken);

    Task<SubjectChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long subjectId,
        SubjectInput input,
        CancellationToken cancellationToken);
}
