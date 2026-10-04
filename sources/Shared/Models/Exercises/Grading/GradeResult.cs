using Shared.Enums;

namespace Shared.Models.Exercises.Grading;

/// <summary>
/// Result of grading one answer (LP-111). Gaps and pairs count one point each; <see cref="Parts"/> tells which part
/// was right (one entry for single-part questions). <see cref="IsCorrect"/> only when every part is right.
/// </summary>
public sealed record GradeResult(bool IsCorrect, int Points, int MaxPoints, GradeHint Hint, IReadOnlyList<bool> Parts)
{
    public static GradeResult FromParts(IReadOnlyList<bool> parts, GradeHint hint = GradeHint.None)
    {
        var points = parts.Count(part => part);
        return new GradeResult(parts.Count > 0 && points == parts.Count, points, parts.Count, hint, parts);
    }

    public static GradeResult Single(bool isCorrect, GradeHint hint = GradeHint.None) => FromParts([isCorrect], hint);

    public static GradeResult Wrong(int partCount) => FromParts(Enumerable.Repeat(false, partCount).ToArray());
}
