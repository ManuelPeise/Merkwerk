using System.Text.Json.Serialization;

namespace Shared.Models.Exercises.Questions;

/// <summary>
/// What the child sees (ADR 005). One record per question type, told apart by <c>type</c>; a new type is a new record
/// plus its <see cref="QuestionSolution"/> and grader (LP-111) – no migration.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = QuestionTypes.PropertyName)]
[JsonDerivedType(typeof(ChoicePayload), QuestionTypes.Choice)]
[JsonDerivedType(typeof(TextPayload), QuestionTypes.Text)]
[JsonDerivedType(typeof(ClozePayload), QuestionTypes.Cloze)]
[JsonDerivedType(typeof(MatchPayload), QuestionTypes.Match)]
[JsonDerivedType(typeof(FlashcardPayload), QuestionTypes.Flashcard)]
public abstract record QuestionPayload
{
    /// <summary>The task or instruction, e.g. "Was heißt 'dog'?" – also read aloud.</summary>
    public required string Prompt { get; init; }
}
