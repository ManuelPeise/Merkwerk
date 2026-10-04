using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>Self-assessment: "knew it" counts as right (Leitner, LP-120).</summary>
internal sealed class FlashcardGrader : QuestionGrader<FlashcardPayload, FlashcardSolution, FlashcardResponse>
{
    public override string Type => QuestionTypes.Flashcard;

    protected override GradeResult Grade(FlashcardPayload payload, FlashcardSolution solution, FlashcardResponse response) =>
        GradeResult.Single(response.Knew);
}
