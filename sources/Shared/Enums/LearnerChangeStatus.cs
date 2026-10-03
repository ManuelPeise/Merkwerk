namespace Shared.Enums;

public enum LearnerChangeStatus
{
    Success,

    /// <summary>404 – unknown here, belongs to another family, or the acting user is no admin.</summary>
    NotFound,

    /// <summary>400, see Errors.</summary>
    Invalid,

    /// <summary>A family has at most 10 (<c>LearnerRules.MaxPerOrganization</c>) children (409).</summary>
    LimitReached,
}
