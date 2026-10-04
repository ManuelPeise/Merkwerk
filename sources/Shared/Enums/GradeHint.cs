namespace Shared.Enums;

/// <summary>Extra feedback of a grader (LP-111) besides right or wrong.</summary>
public enum GradeHint
{
    None,

    /// <summary>Wrong, but only one typo away from an accepted answer – the player can ask for another try.</summary>
    AlmostRight,
}
