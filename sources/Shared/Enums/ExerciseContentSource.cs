namespace Shared.Enums;

/// <summary>Where the tasks of an exercise come from (LP-110).</summary>
public enum ExerciseContentSource
{
    /// <summary>Questions written by hand (LP-110/LP-112).</summary>
    Questions,

    /// <summary>A word list (LP-140) – not supported yet.</summary>
    WordList,

    /// <summary>A generator with parameters, e.g. arithmetic (LP-131) – not supported yet.</summary>
    Generator,
}
