namespace Shared.Models.Exercises.Questions;

/// <summary>Type discriminators of questions (ADR 005) – also the registry keys of graders (LP-111) and client components.</summary>
public static class QuestionTypes
{
    public const string PropertyName = "type";
    public const string Choice = "choice";
    public const string Text = "text";
    public const string Cloze = "cloze";
    public const string Match = "match";
    public const string Flashcard = "flashcard";

    public static readonly IReadOnlyList<string> All = [Choice, Text, Cloze, Match, Flashcard];
}
