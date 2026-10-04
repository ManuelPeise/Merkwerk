namespace Shared.Enums;

public enum ExerciseState
{
    /// <summary>Never published – cannot be assigned yet.</summary>
    Draft,

    /// <summary>Has at least one version; the draft may have unpublished changes.</summary>
    Published,

    /// <summary>Hidden from the list of active exercises; its versions stay valid for running assignments.</summary>
    Archived,
}
