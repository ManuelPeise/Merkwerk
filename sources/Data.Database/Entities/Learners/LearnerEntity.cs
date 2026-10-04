using Data.Database.Entities.Base;

namespace Data.Database.Entities.Learners;

/// <summary>
/// A child profile (LP-105). Deliberately minimal: first name or nickname, grade and a built-in avatar – no photo,
/// no birthday, no surname (privacy, ADR 006).
/// </summary>
public sealed class LearnerEntity : AOrganizationEntityBase
{
    public const int DisplayNameMaxLength = 30;
    public const int AvatarIdMaxLength = 30;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>1–4.</summary>
    public int Grade { get; set; }

    /// <summary>One of the built-in avatars (Web.Client/src/assets/avatars).</summary>
    public string AvatarId { get; set; } = string.Empty;
}
