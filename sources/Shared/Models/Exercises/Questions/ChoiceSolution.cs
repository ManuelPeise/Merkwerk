namespace Shared.Models.Exercises.Questions;

/// <summary>Indexes into <see cref="ChoicePayload.Options"/>; exactly one unless several answers are allowed.</summary>
public sealed record ChoiceSolution : QuestionSolution
{
    public required IReadOnlyList<int> CorrectIndexes { get; init; }
}
