using System.Text.Json.Serialization;

namespace Shared.Models.Exercises.Questions;

/// <summary>The correct answer and the tolerance rules of a question (ADR 005; graded in LP-111).</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = QuestionTypes.PropertyName)]
[JsonDerivedType(typeof(ChoiceSolution), QuestionTypes.Choice)]
[JsonDerivedType(typeof(TextSolution), QuestionTypes.Text)]
[JsonDerivedType(typeof(ClozeSolution), QuestionTypes.Cloze)]
[JsonDerivedType(typeof(MatchSolution), QuestionTypes.Match)]
[JsonDerivedType(typeof(FlashcardSolution), QuestionTypes.Flashcard)]
public abstract record QuestionSolution;
