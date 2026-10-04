namespace Shared.Models.Exercises.Questions;

/// <summary>For every left entry the index of its right partner (a permutation).</summary>
public sealed record MatchSolution : QuestionSolution
{
    public required IReadOnlyList<int> RightIndexForLeft { get; init; }
}
