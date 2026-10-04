namespace Shared.Enums;

public enum SubjectChangeStatus
{
    Success,

    /// <summary>404 – unknown subject, or the acting user is no admin of the family.</summary>
    NotFound,

    /// <summary>400, see Errors (name missing, too long or taken; color, icon or language not in the choice).</summary>
    Invalid,
}
