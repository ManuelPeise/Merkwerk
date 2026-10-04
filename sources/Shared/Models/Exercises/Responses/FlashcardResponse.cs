namespace Shared.Models.Exercises.Responses;

/// <summary>Self-assessment after turning the card: did the child know the back?</summary>
public sealed record FlashcardResponse : QuestionResponse
{
    public bool Knew { get; init; }
}
