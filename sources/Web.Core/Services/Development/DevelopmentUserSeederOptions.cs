namespace Web.Core.Services.Development;

/// <summary>Development only (section "DevelopmentSeed"): one confirmed adult account to sign in with before LP-105.</summary>
public sealed class DevelopmentUserSeederOptions
{
    public const string SectionName = "DevelopmentSeed";

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string DisplayName { get; set; } = "Eltern";
}
