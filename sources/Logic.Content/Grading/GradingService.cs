using Logic.Shared.Interfaces;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>Picks the grader by the payload's type (registry of all <see cref="IQuestionGrader"/>s in DI).</summary>
internal sealed class GradingService : IGradingService
{
    private readonly Dictionary<Type, IQuestionGrader> _graders;

    public GradingService(IEnumerable<IQuestionGrader> graders)
    {
        _graders = graders.ToDictionary(g => g.PayloadType);
    }

    /// <summary>Question types with a grader – the tests compare them with QuestionTypes.All.</summary>
    public IReadOnlyCollection<string> Types => _graders.Values.Select(g => g.Type).ToList();

    public GradeResult Grade(QuestionContent question, QuestionResponse response)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(response);

        return _graders.TryGetValue(question.Payload.GetType(), out var grader)
            ? grader.Grade(question, response)
            : throw new InvalidOperationException($"No grader for {question.Payload.GetType().Name}.");
    }
}
