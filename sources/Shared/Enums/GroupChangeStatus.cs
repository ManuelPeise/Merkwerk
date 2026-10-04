namespace Shared.Enums;

public enum GroupChangeStatus
{
    Success,

    /// <summary>404 – unknown here, belongs to another family, or the acting user is no admin.</summary>
    NotFound,

    /// <summary>400, see Errors (name missing, too long or taken; a child of another family).</summary>
    Invalid,

    /// <summary>A family has at most 20 (<c>GroupRules.MaxPerOrganization</c>) groups (409).</summary>
    LimitReached,
}
