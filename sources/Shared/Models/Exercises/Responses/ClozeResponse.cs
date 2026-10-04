namespace Shared.Models.Exercises.Responses;

/// <summary>One entry per gap, in the order of the gaps.</summary>
public sealed record ClozeResponse : QuestionResponse
{
    public IReadOnlyList<string> Gaps { get; init; } = [];
}
