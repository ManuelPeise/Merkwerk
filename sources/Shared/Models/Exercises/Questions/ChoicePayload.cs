namespace Shared.Models.Exercises.Questions;

/// <summary>Pick one or several of 2–8 options.</summary>
public sealed record ChoicePayload : QuestionPayload
{
    public required IReadOnlyList<string> Options { get; init; }

    public bool MultipleAnswers { get; init; }
}
