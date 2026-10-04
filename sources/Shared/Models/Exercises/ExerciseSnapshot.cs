using Shared.Enums;

namespace Shared.Models.Exercises;

/// <summary>
/// A published version, frozen (ADR 005): stored as one JSON column, never changed. Assignments (LP-114) and attempts
/// point to <c>(exercise, version number)</c>, so editing the draft never changes what a child is working on.
/// </summary>
public sealed record ExerciseSnapshot(
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    IReadOnlyList<QuestionContent> Questions);
