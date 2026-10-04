namespace Shared.Models.Exercises.Questions;

/// <summary>Accepted answers per gap, in the order of the gaps.</summary>
public sealed record ClozeSolution : QuestionSolution
{
    public required IReadOnlyList<IReadOnlyList<string>> Gaps { get; init; }

    public bool CaseSensitive { get; init; }
}
