using System.Text.Json.Serialization;

namespace Shared.Models.Exercises.Generators;

/// <summary>
/// Parameters of an exercise whose tasks are generated (contentSource "generator", LP-131). Only the parameters are
/// stored; every attempt generates its own tasks from a seed (LP-115).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = GeneratorTypes.PropertyName)]
[JsonDerivedType(typeof(ArithmeticSettings), GeneratorTypes.Arithmetic)]
public abstract record GeneratorSettings
{
    /// <summary>Number of tasks per attempt, 1–50.</summary>
    public int TaskCount { get; init; } = 10;
}
