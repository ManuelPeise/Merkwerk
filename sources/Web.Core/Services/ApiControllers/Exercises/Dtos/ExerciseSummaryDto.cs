using Shared.Enums;
using Shared.Models.Exercises;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>latestVersion 0 = never published; enums travel as camelCase strings.</summary>
public sealed record ExerciseSummaryDto(
    long Id,
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    ExerciseState State,
    int LatestVersion,
    bool HasUnpublishedChanges,
    int QuestionCount)
{
    public static ExerciseSummaryDto From(ExerciseSummary summary) => new(
        summary.Id,
        summary.Title,
        summary.SubjectId,
        summary.Grade,
        summary.ContentSource,
        summary.State,
        summary.LatestVersion,
        summary.HasUnpublishedChanges,
        summary.QuestionCount);
}
