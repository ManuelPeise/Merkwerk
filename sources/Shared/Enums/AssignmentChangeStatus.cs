namespace Shared.Enums;

public enum AssignmentChangeStatus
{
    Success,

    /// <summary>404 – exercise or assignment unknown here, of another family, or the acting user is no member.</summary>
    NotFound,

    /// <summary>400, see Errors.</summary>
    Invalid,
}
