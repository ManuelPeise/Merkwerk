namespace Data.Database.Entities.Base;

/// <summary>
/// Base of everything owned by a family or school (ADR 007). A global query filter only returns rows of the
/// current organization; services additionally check ownership, because IDs are sequential and guessable.
/// </summary>
public abstract class AOrganizationEntityBase : AEntityBase
{
    public long OrganizationId { get; set; }
}
