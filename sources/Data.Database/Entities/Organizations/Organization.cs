using Data.Database.Entities.Base;

namespace Data.Database.Entities.Organizations;

/// <summary>A family (later also a school). Root of tenant isolation – therefore not organization-scoped itself.</summary>
public sealed class Organization : AEntityBase
{
    public const int NameMaxLength = 100;

    public string Name { get; set; } = string.Empty;
}
