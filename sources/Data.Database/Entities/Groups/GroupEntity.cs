using Data.Database.Entities.Base;

namespace Data.Database.Entities.Groups;

/// <summary>
/// Children of a family grouped for assigning exercises together (LP-108; assignments to a group in LP-114). Name unique
/// within the family; the children hang on <see cref="GroupLearnerEntity"/>.
/// </summary>
public sealed class GroupEntity : AOrganizationEntityBase
{
    public const int NameMaxLength = 50;

    public string Name { get; set; } = string.Empty;

    public List<GroupLearnerEntity> Learners { get; set; } = [];
}
