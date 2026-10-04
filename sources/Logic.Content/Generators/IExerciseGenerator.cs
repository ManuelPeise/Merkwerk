using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;

namespace Logic.Content.Generators;

/// <summary>One generator per settings type (registry key: <see cref="SettingsType"/>).</summary>
internal interface IExerciseGenerator
{
    Type SettingsType { get; }

    Dictionary<string, string[]> Validate(GeneratorSettings settings);

    IReadOnlyList<QuestionContent> Generate(GeneratorSettings settings, int seed);
}
