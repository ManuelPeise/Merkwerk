namespace Shared.Models.Exercises.Responses;

/// <summary>Indexes of the options the child selected (order does not matter).</summary>
public sealed record ChoiceResponse : QuestionResponse
{
    public IReadOnlyList<int> SelectedIndexes { get; init; } = [];
}
