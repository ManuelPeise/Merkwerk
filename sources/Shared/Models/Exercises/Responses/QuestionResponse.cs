using System.Text.Json.Serialization;
using Shared.Models.Exercises.Questions;

namespace Shared.Models.Exercises.Responses;

/// <summary>
/// What the child answered (LP-111). Same discriminator as payload and solution (<see cref="QuestionTypes"/>), so a
/// response of the wrong type is recognised and graded as wrong.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = QuestionTypes.PropertyName)]
[JsonDerivedType(typeof(ChoiceResponse), QuestionTypes.Choice)]
[JsonDerivedType(typeof(TextResponse), QuestionTypes.Text)]
[JsonDerivedType(typeof(ClozeResponse), QuestionTypes.Cloze)]
[JsonDerivedType(typeof(MatchResponse), QuestionTypes.Match)]
[JsonDerivedType(typeof(FlashcardResponse), QuestionTypes.Flashcard)]
public abstract record QuestionResponse;
