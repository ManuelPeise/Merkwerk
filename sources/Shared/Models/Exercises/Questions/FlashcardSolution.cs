namespace Shared.Models.Exercises.Questions;

/// <summary>The back of the card.</summary>
public sealed record FlashcardSolution : QuestionSolution
{
    public required string Back { get; init; }
}
