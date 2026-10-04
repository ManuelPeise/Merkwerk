using System.ComponentModel.DataAnnotations;
using Shared.Enums;
using Shared.Models.Exercises;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>The whole draft. Rules per question type are checked by the service (field errors "questions[n]").</summary>
public sealed record SaveExerciseRequestDto(
    [Required, MaxLength(100)] string Title,
    [Range(1, long.MaxValue)] long SubjectId,
    [Range(1, 4)] int Grade,
    ExerciseContentSource ContentSource,
    [Required, MaxLength(50)] IReadOnlyList<QuestionDto> Questions)
{
    public ExerciseInput ToInput() =>
        new(Title, SubjectId, Grade, ContentSource, Questions.Select(q => q.ToContent()).ToList());
}
