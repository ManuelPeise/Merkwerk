using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>One point per left entry that sits next to its partner.</summary>
internal sealed class MatchGrader : QuestionGrader<MatchPayload, MatchSolution, MatchResponse>
{
    public override string Type => QuestionTypes.Match;

    protected override int PartCount(MatchPayload payload, MatchSolution solution) => solution.RightIndexForLeft.Count;

    protected override GradeResult Grade(MatchPayload payload, MatchSolution solution, MatchResponse response) =>
        GradeResult.FromParts(solution.RightIndexForLeft
            .Select((right, left) => left < response.RightIndexForLeft.Count && response.RightIndexForLeft[left] == right)
            .ToArray());
}
