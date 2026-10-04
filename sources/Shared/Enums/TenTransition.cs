namespace Shared.Enums;

/// <summary>
/// Crossing a ten in addition and subtraction (LP-131): 8 + 5 or 13 − 5 cross it, 7 + 3 or 13 − 3 do not.
/// </summary>
public enum TenTransition
{
    Any,
    Without,
    With,
}
