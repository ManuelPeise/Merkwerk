namespace Shared.Models.Exercises.Responses;

/// <summary>The typed word, sentence or number.</summary>
public sealed record TextResponse : QuestionResponse
{
    public string Text { get; init; } = string.Empty;
}
