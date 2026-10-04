namespace Shared.Enums;

public enum ExerciseChangeStatus
{
    Success,

    /// <summary>404 – unknown here, belongs to another family, or the acting user is no member.</summary>
    NotFound,

    /// <summary>400, see Errors.</summary>
    Invalid,
}
