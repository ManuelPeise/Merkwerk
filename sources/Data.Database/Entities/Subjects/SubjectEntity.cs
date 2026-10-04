using Data.Database.Entities.Base;

namespace Data.Database.Entities.Subjects;

/// <summary>
/// A school subject (German, English, maths, later any other language). Instance-wide, not organization-scoped:
/// shared and public exercises (LP-201) must mean the same subject in every family.
/// </summary>
public sealed class SubjectEntity : AEntityBase
{
    public const int NameMaxLength = 50;
    public const int LanguageCodeMaxLength = 10;
    public const int ColorMaxLength = 50;
    public const int IconMaxLength = 50;

    public string Name { get; set; } = string.Empty;

    /// <summary>BCP 47 language code used for reading aloud and word lists, e.g. <c>de</c>, <c>en</c>, <c>fr</c>.</summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>Design token key, e.g. <c>subject.german</c> (shared/design-tokens/tokens.json) – never a hex value.</summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>Icon key the UI maps to an icon, e.g. <c>german</c>, <c>english</c>, <c>math</c>.</summary>
    public string Icon { get; set; } = string.Empty;
}
