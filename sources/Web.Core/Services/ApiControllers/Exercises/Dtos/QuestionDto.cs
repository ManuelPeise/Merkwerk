using Shared.Models.Exercises;
using Shared.Models.Exercises.Questions;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>Payload and solution with a <c>type</c> discriminator ("choice", "text", "cloze", "match", "flashcard").</summary>
public sealed record QuestionDto(QuestionPayload Payload, QuestionSolution Solution)
{
    public static QuestionDto From(QuestionContent content) => new(content.Payload, content.Solution);

    public QuestionContent ToContent() => new(Payload, Solution);
}
