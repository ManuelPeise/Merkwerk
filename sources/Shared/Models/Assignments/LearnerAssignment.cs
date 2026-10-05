using Shared.Models.Exercises.Generators;

namespace Shared.Models.Assignments;

/// <summary>
/// What a child has to do (for the children's API, LP-115): one entry per exercise, even if the child got it directly
/// and through groups. <see cref="Generator"/> holds the settings to use – the assignment's or the exercise's own.
/// </summary>
public sealed record LearnerAssignment(
    long AssignmentId,
    long ExerciseId,
    DateOnly? DueDate,
    bool AllowDotArray,
    bool AllowTimesTableMatrix,
    GeneratorSettings? Generator);
