namespace Shared.Models.Exercises.Questions;

/// <summary>
/// Text with gaps: <see cref="Parts"/> are the text pieces, a gap sits between two neighbours
/// ("Der ", " ist rot." → "Der ___ ist rot.").
/// </summary>
public sealed record ClozePayload : QuestionPayload
{
    public required IReadOnlyList<string> Parts { get; init; }
}
