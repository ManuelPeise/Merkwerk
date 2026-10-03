using Data.Database.Entities.Base;

namespace Data.Database.Entities.Organizations;

/// <summary>An adult belongs to a family (or later a school) with a role (LP-105, LP-107). The role hangs here, not on the user.</summary>
public sealed class Membership : AOrganizationEntityBase
{
    public long UserId { get; set; }

    public OrganizationRole Role { get; set; }

    /// <summary>Created the organization in the first-run setup. Cannot be removed.</summary>
    public bool IsOwner { get; set; }
}
