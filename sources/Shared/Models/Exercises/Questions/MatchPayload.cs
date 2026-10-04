namespace Shared.Models.Exercises.Questions;

/// <summary>Match each entry on the left to one on the right (2–8 pairs; the client shuffles the right side).</summary>
public sealed record MatchPayload : QuestionPayload
{
    public required IReadOnlyList<string> Left { get; init; }

    public required IReadOnlyList<string> Right { get; init; }
}
