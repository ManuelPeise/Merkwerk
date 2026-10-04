namespace Shared.Enums;

/// <summary>Tasks with a missing operand like "7 + □ = 12" (LP-131).</summary>
public enum PlaceholderMode
{
    None,
    Mixed,
    Only,
}
