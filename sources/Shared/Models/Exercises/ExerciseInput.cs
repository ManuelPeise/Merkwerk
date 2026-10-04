using Shared.Enums;

namespace Shared.Models.Exercises;

/// <summary>The whole draft as the editor saves it: questions in display order (replaces the previous ones).</summary>
public sealed record ExerciseInput(
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    IReadOnlyList<QuestionContent> Questions);
