using Shared.Models.Exercises.Generators;

namespace Shared.Models.Assignments;

/// <summary>
/// Assigns an exercise to several children and groups at once (LP-114). Children or groups that already have this
/// exercise keep their assignment, which takes over the new options.
/// </summary>
public sealed record AssignmentInput(
    long ExerciseId,
    IReadOnlyList<long> LearnerIds,
    IReadOnlyList<long> GroupIds,
    DateOnly? DueDate,
    bool AllowDotArray,
    bool AllowTimesTableMatrix,
    GeneratorSettings? Generator = null);
