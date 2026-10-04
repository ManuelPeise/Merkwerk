using Shared.Enums;

namespace Shared.Models.Exercises;

/// <summary>An exercise in the list. <see cref="LatestVersion"/> 0 = never published.</summary>
public sealed record ExerciseSummary(
    long Id,
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    ExerciseState State,
    int LatestVersion,
    bool HasUnpublishedChanges,
    int QuestionCount);
