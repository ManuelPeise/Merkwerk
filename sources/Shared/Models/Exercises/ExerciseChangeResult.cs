using Shared.Enums;

namespace Shared.Models.Exercises;

public sealed record ExerciseChangeResult(
    ExerciseChangeStatus Status,
    ExerciseSummary? Exercise = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
