using Shared.Enums;
using Shared.Models.Exercises.Generators;

namespace Shared.Models.Exercises;

/// <summary>
/// The whole draft as the editor saves it: questions in display order (replaces the previous ones), or – for
/// <see cref="ExerciseContentSource.Generator"/> – the generator settings and no questions (LP-131).
/// </summary>
public sealed record ExerciseInput(
    string Title,
    long SubjectId,
    int Grade,
    ExerciseContentSource ContentSource,
    IReadOnlyList<QuestionContent> Questions,
    GeneratorSettings? Generator = null);
