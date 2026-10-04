using Shared.Models.Exercises.Questions;

namespace Shared.Models.Exercises;

/// <summary>One question as the editor sends it and as a version freezes it.</summary>
public sealed record QuestionContent(QuestionPayload Payload, QuestionSolution Solution);
