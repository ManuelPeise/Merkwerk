using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Responses;

namespace Logic.Shared.Interfaces;

/// <summary>
/// Grades an answer against a question (LP-111). Pure and synchronous: no database, no clock. The server's result is
/// authoritative (ADR 001); the JSON cases in shared/grading-cases are the contract for a later client-side port.
/// </summary>
public interface IGradingService
{
    /// <summary>
    /// A response of another question type is graded as wrong. Throws if payload and solution do not belong together
    /// (the editor's validation, LP-110, prevents that).
    /// </summary>
    GradeResult Grade(QuestionContent question, QuestionResponse response);
}
