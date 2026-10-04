namespace Shared.Models.Exercises;

/// <summary>The draft for the editor: summary plus the questions in display order.</summary>
public sealed record ExerciseDetail(ExerciseSummary Summary, IReadOnlyList<QuestionContent> Questions);
