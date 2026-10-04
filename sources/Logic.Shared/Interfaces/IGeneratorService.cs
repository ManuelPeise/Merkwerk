using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;

namespace Logic.Shared.Interfaces;

/// <summary>
/// Generates tasks from generator settings (LP-131). Deterministic: the same settings and seed always give the same
/// tasks, so an attempt only has to remember its seed (LP-115).
/// </summary>
public interface IGeneratorService
{
    /// <summary>Field errors with keys like "generator.numberRange"; empty when the settings can be used.</summary>
    IReadOnlyDictionary<string, string[]> Validate(GeneratorSettings? settings);

    /// <summary>Number questions (text, inputKind number). Throws for invalid settings – validate first.</summary>
    IReadOnlyList<QuestionContent> Generate(GeneratorSettings settings, int seed);

    /// <summary>A fresh random seed for a new attempt or preview.</summary>
    int CreateSeed();
}
