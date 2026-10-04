using Shared.Enums;

namespace Shared.Models.Exercises.Generators;

/// <summary>Basic arithmetic (LP-131). Results are never negative; division never leaves a remainder.</summary>
public sealed record ArithmeticSettings : GeneratorSettings
{
    /// <summary>One or several, mixed at random.</summary>
    public IReadOnlyList<ArithmeticOperation> Operations { get; init; } = [ArithmeticOperation.Add];

    /// <summary>Largest number in a task: 10, 20, 100, 1000 or any value from 5 to 1000.</summary>
    public int NumberRange { get; init; } = 20;

    /// <summary>Only for addition and subtraction, from number range 20.</summary>
    public TenTransition TenTransition { get; init; }

    public PlaceholderMode Placeholder { get; init; }
}
