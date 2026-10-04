using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>Checks the types once, so each grader only implements the rules of its question type.</summary>
internal abstract class QuestionGrader<TPayload, TSolution, TResponse> : IQuestionGrader
    where TPayload : QuestionPayload
    where TSolution : QuestionSolution
    where TResponse : QuestionResponse
{
    public abstract string Type { get; }

    public Type PayloadType => typeof(TPayload);

    public GradeResult Grade(QuestionContent question, QuestionResponse response)
    {
        if (question.Payload is not TPayload payload || question.Solution is not TSolution solution)
        {
            throw new ArgumentException($"Payload and solution must both be of type '{Type}'.", nameof(question));
        }

        return response is TResponse typed
            ? Grade(payload, solution, typed)
            : GradeResult.Wrong(PartCount(payload, solution));
    }

    /// <summary>Number of points the question is worth (gaps, pairs, otherwise 1).</summary>
    protected virtual int PartCount(TPayload payload, TSolution solution) => 1;

    protected abstract GradeResult Grade(TPayload payload, TSolution solution, TResponse response);
}
