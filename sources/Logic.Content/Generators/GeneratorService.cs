using System.Security.Cryptography;
using Logic.Shared.Interfaces;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;

namespace Logic.Content.Generators;

/// <summary>Picks the generator by the settings' type (registry of all <see cref="IExerciseGenerator"/>s in DI).</summary>
internal sealed class GeneratorService : IGeneratorService
{
    private readonly Dictionary<Type, IExerciseGenerator> _generators;

    public GeneratorService(IEnumerable<IExerciseGenerator> generators)
    {
        _generators = generators.ToDictionary(g => g.SettingsType);
    }

    public IReadOnlyDictionary<string, string[]> Validate(GeneratorSettings? settings)
    {
        if (settings is null)
        {
            return new Dictionary<string, string[]> { ["generator"] = ["Generator settings are required."] };
        }

        return _generators.TryGetValue(settings.GetType(), out var generator)
            ? generator.Validate(settings)
            : new Dictionary<string, string[]> { ["generator"] = ["Unknown generator."] };
    }

    public IReadOnlyList<QuestionContent> Generate(GeneratorSettings settings, int seed)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!_generators.TryGetValue(settings.GetType(), out var generator))
        {
            throw new InvalidOperationException($"No generator for {settings.GetType().Name}.");
        }

        var errors = generator.Validate(settings);
        return errors.Count == 0
            ? generator.Generate(settings, seed)
            : throw new ArgumentException("Invalid generator settings: " + string.Join(", ", errors.Keys), nameof(settings));
    }

    public int CreateSeed() => RandomNumberGenerator.GetInt32(int.MaxValue);
}
