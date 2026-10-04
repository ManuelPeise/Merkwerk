using Shared.Models.Exercises;

namespace Logic.Shared.Interfaces;

/// <summary>
/// Exercises of a family (LP-110, ADR 005): every adult creates, edits, publishes and archives. Publishing freezes the
/// draft as version 1, 2, …; versions never change.
/// </summary>
public interface IExerciseService
{
    /// <summary>All exercises of the family (archived ones included, see State); null if the acting user is no member.</summary>
    Task<IReadOnlyList<ExerciseSummary>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    /// <summary>The draft with its questions; null if unknown, of another family, or the acting user is no member.</summary>
    Task<ExerciseDetail?> GetAsync(long organizationId, long actingUserId, long exerciseId, CancellationToken cancellationToken);

    Task<ExerciseChangeResult> CreateAsync(
        long organizationId,
        long actingUserId,
        ExerciseInput input,
        CancellationToken cancellationToken);

    /// <summary>Saves the whole draft; the questions are replaced in the given order.</summary>
    Task<ExerciseChangeResult> UpdateAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        ExerciseInput input,
        CancellationToken cancellationToken);

    /// <summary>Freezes the draft as the next version (nothing happens if it has no unpublished changes).</summary>
    Task<ExerciseChangeResult> PublishAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        CancellationToken cancellationToken);

    /// <summary>Archives or restores; versions stay valid for running assignments.</summary>
    Task<ExerciseChangeResult> SetArchivedAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        bool archived,
        CancellationToken cancellationToken);

    /// <summary>A published version; null if it does not exist in this family.</summary>
    Task<ExerciseSnapshot?> GetVersionAsync(
        long organizationId,
        long actingUserId,
        long exerciseId,
        int versionNumber,
        CancellationToken cancellationToken);
}
