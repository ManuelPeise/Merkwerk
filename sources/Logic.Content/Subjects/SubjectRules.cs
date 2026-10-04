using Data.Database.Entities.Subjects;

namespace Logic.Content.Subjects;

/// <summary>
/// The fixed choice for subjects (LP-109): colors are design-token keys (shared/design-tokens/tokens.json, mirrored in
/// Web.Client's theme), icons are keys the web client maps to icons. Free values are not allowed, so a subject looks
/// the same in the adults' and the children's area.
/// </summary>
public static class SubjectRules
{
    public const int NameMaxLength = SubjectEntity.NameMaxLength;

    /// <summary>Token keys under <c>color.subject</c>; white text and icons on them have a contrast of at least 4.5:1.</summary>
    public static readonly IReadOnlyList<string> Colors =
    [
        "subject.german",
        "subject.english",
        "subject.math",
        "subject.purple",
        "subject.orange",
        "subject.magenta",
        "subject.slate",
        "subject.olive",
    ];

    /// <summary>Icon keys (mapped in Web.Client/src/lib/subjects/subjectStyles.ts).</summary>
    public static readonly IReadOnlyList<string> Icons =
    [
        "german",
        "english",
        "math",
        "book",
        "music",
        "science",
        "art",
        "sport",
        "globe",
        "language",
        "puzzle",
        "star",
    ];

    /// <summary>Languages for reading aloud and word lists (BCP 47).</summary>
    public static readonly IReadOnlyList<string> LanguageCodes = ["de", "en", "fr", "es", "it"];
}
