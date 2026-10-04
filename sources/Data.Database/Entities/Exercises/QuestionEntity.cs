using Data.Database.Entities.Base;
using Shared.Models.Exercises.Questions;

namespace Data.Database.Entities.Exercises;

/// <summary>A question of an exercise draft. Payload and solution are JSON columns with polymorphic types (ADR 005).</summary>
public sealed class QuestionEntity : AOrganizationEntityBase
{
    public long ExerciseId { get; set; }

    /// <summary>0-based display order.</summary>
    public int Position { get; set; }

    public QuestionPayload Payload { get; set; } = null!;

    public QuestionSolution Solution { get; set; } = null!;
}
