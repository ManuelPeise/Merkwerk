namespace Shared.Models.Exercises.Questions;

/// <summary>Flashcard: front shown, child recalls the back and rates itself (Leitner, LP-120).</summary>
public sealed record FlashcardPayload : QuestionPayload
{
    public required string Front { get; init; }
}
