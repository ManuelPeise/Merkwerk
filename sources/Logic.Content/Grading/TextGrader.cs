using Shared.Enums;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>
/// Numbers: equal within <see cref="TextSolution.NumberTolerance"/>. Text: any accepted answer after normalising;
/// with <see cref="TextSolution.AllowTypo"/> one typo is wrong but "almost right".
/// </summary>
internal sealed class TextGrader : QuestionGrader<TextPayload, TextSolution, TextResponse>
{
    public override string Type => QuestionTypes.Text;

    protected override GradeResult Grade(TextPayload payload, TextSolution solution, TextResponse response) =>
        payload.InputKind == TextInputKind.Number ? GradeNumber(solution, response) : GradeText(solution, response);

    private static GradeResult GradeNumber(TextSolution solution, TextResponse response)
    {
        var tolerance = solution.NumberTolerance ?? 0m;

        return GradeResult.Single(
            AnswerText.TryParseNumber(response.Text, out var given)
            && solution.AcceptedAnswers.Any(answer =>
                AnswerText.TryParseNumber(answer, out var expected) && Math.Abs(given - expected) <= tolerance));
    }

    private static GradeResult GradeText(TextSolution solution, TextResponse response)
    {
        var given = AnswerText.Normalize(response.Text, solution.CaseSensitive);

        if (given.Length == 0)
        {
            return GradeResult.Single(false);
        }

        var accepted = solution.AcceptedAnswers.Select(a => AnswerText.Normalize(a, solution.CaseSensitive)).ToList();

        if (accepted.Contains(given, StringComparer.Ordinal))
        {
            return GradeResult.Single(true);
        }

        return solution.AllowTypo && accepted.Any(answer => AnswerText.IsOneTypoAway(answer, given))
            ? GradeResult.Single(false, GradeHint.AlmostRight)
            : GradeResult.Single(false);
    }
}
