using Shared.Models.Assignments;
using Shared.Models.Exercises.Generators;

namespace Web.Core.Services.ApiControllers.Assignments.Dtos;

/// <summary>
/// To exactly one child (learnerId) or one group (groupId). dueDate as "yyyy-MM-dd" or null; generator null = the
/// exercise's own settings.
/// </summary>
public sealed record AssignmentDto(
    long Id,
    long ExerciseId,
    long? LearnerId,
    long? GroupId,
    DateOnly? DueDate,
    bool AllowDotArray,
    bool AllowTimesTableMatrix,
    GeneratorSettings? Generator,
    DateTime AssignedAt)
{
    public static AssignmentDto From(AssignmentInfo info) => new(
        info.Id,
        info.ExerciseId,
        info.LearnerId,
        info.GroupId,
        info.DueDate,
        info.AllowDotArray,
        info.AllowTimesTableMatrix,
        info.Generator,
        info.AssignedAt);
}
