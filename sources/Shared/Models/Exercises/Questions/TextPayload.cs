using Shared.Enums;

namespace Shared.Models.Exercises.Questions;

/// <summary>Type a word, a sentence or a number.</summary>
public sealed record TextPayload : QuestionPayload
{
    public TextInputKind InputKind { get; init; }
}
