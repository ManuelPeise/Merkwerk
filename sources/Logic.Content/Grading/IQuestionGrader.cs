using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Grading;

/// <summary>One grader per question type (registry key: <see cref="Type"/> from QuestionTypes).</summary>
internal interface IQuestionGrader
{
    string Type { get; }

    Type PayloadType { get; }

    GradeResult Grade(QuestionContent question, QuestionResponse response);
}
