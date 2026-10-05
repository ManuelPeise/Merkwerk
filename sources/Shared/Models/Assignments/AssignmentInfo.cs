using Shared.Models.Exercises.Generators;

namespace Shared.Models.Assignments;

/// <summary>
/// An assignment as adults see it: to exactly one child (<see cref="LearnerId"/>) or one group (<see cref="GroupId"/>).
/// <see cref="Generator"/> null = the exercise's own settings.
/// </summary>
public sealed record AssignmentInfo(
    long Id,
    long ExerciseId,
    long? LearnerId,
    long? GroupId,
    DateOnly? DueDate,
    bool AllowDotArray,
    bool AllowTimesTableMatrix,
    GeneratorSettings? Generator,
    DateTime AssignedAt);
