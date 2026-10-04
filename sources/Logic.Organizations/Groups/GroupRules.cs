using Data.Database.Entities.Groups;

namespace Logic.Organizations.Groups;

/// <summary>Rules for groups of children (LP-108).</summary>
public static class GroupRules
{
    public const int MaxPerOrganization = 20;
    public const int NameMaxLength = GroupEntity.NameMaxLength;
}
