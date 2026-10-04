using Data.Database.Entities.Base;

namespace Data.Database.Entities.Groups;

/// <summary>A child in a group (n:m, LP-108). Deleted together with the group, the child or the family.</summary>
public sealed class GroupLearnerEntity : AOrganizationEntityBase
{
    public long GroupId { get; set; }

    public long LearnerId { get; set; }
}
