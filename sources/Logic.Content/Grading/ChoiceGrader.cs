using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>Right when exactly the correct options are selected – also with several answers (all or nothing).</summary>
internal sealed class ChoiceGrader : QuestionGrader<ChoicePayload, ChoiceSolution, ChoiceResponse>
{
    public override string Type => QuestionTypes.Choice;

    protected override GradeResult Grade(ChoicePayload payload, ChoiceSolution solution, ChoiceResponse response) =>
        GradeResult.Single(response.SelectedIndexes.ToHashSet().SetEquals(solution.CorrectIndexes));
}
