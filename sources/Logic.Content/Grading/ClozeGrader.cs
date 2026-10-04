using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>One point per gap; a gap is right when it matches one of its accepted answers after normalising.</summary>
internal sealed class ClozeGrader : QuestionGrader<ClozePayload, ClozeSolution, ClozeResponse>
{
    public override string Type => QuestionTypes.Cloze;

    protected override int PartCount(ClozePayload payload, ClozeSolution solution) => solution.Gaps.Count;

    protected override GradeResult Grade(ClozePayload payload, ClozeSolution solution, ClozeResponse response) =>
        GradeResult.FromParts(solution.Gaps
            .Select((accepted, index) => index < response.Gaps.Count && IsRight(accepted, response.Gaps[index], solution))
            .ToArray());

    private static bool IsRight(IReadOnlyList<string> accepted, string? given, ClozeSolution solution)
    {
        var normalized = AnswerText.Normalize(given, solution.CaseSensitive);

        return normalized.Length > 0
            && accepted.Any(answer => AnswerText.Normalize(answer, solution.CaseSensitive) == normalized);
    }
}
