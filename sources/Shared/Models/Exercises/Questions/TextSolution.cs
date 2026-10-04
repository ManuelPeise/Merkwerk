namespace Shared.Models.Exercises.Questions;

/// <summary>Accepted answers and tolerance (LP-111): case, one typo as "almost right", number tolerance.</summary>
public sealed record TextSolution : QuestionSolution
{
    public required IReadOnlyList<string> AcceptedAnswers { get; init; }

    public bool CaseSensitive { get; init; }

    /// <summary>One typo counts as "almost right" (text only).</summary>
    public bool AllowTypo { get; init; }

    /// <summary>Allowed difference for numbers, e.g. 0.01; null = exact.</summary>
    public decimal? NumberTolerance { get; init; }
}
