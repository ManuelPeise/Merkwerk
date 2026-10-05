using System.ComponentModel.DataAnnotations;
using Shared.Models.Assignments;
using Shared.Models.Exercises.Generators;

namespace Web.Core.Services.ApiControllers.Assignments.Dtos;

/// <summary>
/// Children and groups of the own family (together at least one). Those that already have the exercise take the new
/// options. generator only for generator exercises (null = the exercise's settings).
/// </summary>
public sealed record AssignExerciseRequestDto(
    [Range(1, long.MaxValue)] long ExerciseId,
    [Required, MaxLength(30)] IReadOnlyList<long> LearnerIds,
    [Required, MaxLength(30)] IReadOnlyList<long> GroupIds,
    DateOnly? DueDate,
    bool AllowDotArray,
    bool AllowTimesTableMatrix,
    GeneratorSettings? Generator = null)
{
    public AssignmentInput ToInput() =>
        new(ExerciseId, LearnerIds, GroupIds, DueDate, AllowDotArray, AllowTimesTableMatrix, Generator);
}
