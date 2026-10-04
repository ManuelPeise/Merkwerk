using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>A published version exactly as it was frozen.</summary>
public sealed record ExerciseVersionDto(
    long ExerciseId,
    int Number,
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    IReadOnlyList<QuestionDto> Questions,
    GeneratorSettings? Generator)
{
    public static ExerciseVersionDto From(long exerciseId, int number, ExerciseSnapshot snapshot) => new(
        exerciseId,
        number,
        snapshot.Title,
        snapshot.SubjectId,
        snapshot.Grade,
        snapshot.ContentSource,
        snapshot.Questions.Select(QuestionDto.From).ToList(),
        snapshot.Generator);
}
