namespace Shared.Models.Exercises.Responses;

/// <summary>For every left entry the index of the right entry the child put next to it.</summary>
public sealed record MatchResponse : QuestionResponse
{
    public IReadOnlyList<int> RightIndexForLeft { get; init; } = [];
}
