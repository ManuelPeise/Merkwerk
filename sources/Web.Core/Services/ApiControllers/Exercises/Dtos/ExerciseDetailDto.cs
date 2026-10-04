using Shared.Models.Exercises;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

public sealed record ExerciseDetailDto(ExerciseSummaryDto Exercise, IReadOnlyList<QuestionDto> Questions)
{
    public static ExerciseDetailDto From(ExerciseDetail detail) =>
        new(ExerciseSummaryDto.From(detail.Summary), detail.Questions.Select(QuestionDto.From).ToList());
}
