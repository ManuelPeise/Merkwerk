using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>The draft: questions, or for generator exercises the settings (LP-131).</summary>
public sealed record ExerciseDetailDto(
    ExerciseSummaryDto Exercise,
    IReadOnlyList<QuestionDto> Questions,
    GeneratorSettings? Generator)
{
    public static ExerciseDetailDto From(ExerciseDetail detail) =>
        new(ExerciseSummaryDto.From(detail.Summary), detail.Questions.Select(QuestionDto.From).ToList(), detail.Generator);
}
