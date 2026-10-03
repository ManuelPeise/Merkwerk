namespace Logic.Organizations.Learners;

/// <summary>Rules for child profiles (LP-105).</summary>
public static class LearnerRules
{
    public const int MaxPerOrganization = 10;
    public const int MinGrade = 1;
    public const int MaxGrade = 4;

    /// <summary>Same list as Web.Client/src/assets/avatars/avatars.ts.</summary>
    public static readonly IReadOnlyList<string> AvatarIds =
    [
        "fox", "owl", "cat", "dog", "bear", "rabbit", "frog", "lion", "panda", "pig", "mouse", "penguin",
    ];
}
