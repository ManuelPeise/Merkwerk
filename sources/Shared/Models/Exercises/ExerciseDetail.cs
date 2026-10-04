using Shared.Models.Exercises.Generators;

namespace Shared.Models.Exercises;

/// <summary>The draft for the editor: summary plus the questions in display order, or the generator settings (LP-131).</summary>
public sealed record ExerciseDetail(
    ExerciseSummary Summary,
    IReadOnlyList<QuestionContent> Questions,
    GeneratorSettings? Generator = null);
