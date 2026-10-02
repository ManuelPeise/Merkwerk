namespace Logic.Authentication;

/// <summary>
/// LP-006 spike only: one demo account from configuration (section "Spike", Development settings).
/// Replaced by ASP.NET Core Identity accounts in LP-104.
/// </summary>
public sealed class DemoUserOptions
{
    public const string SectionName = "Spike";

    public string? DemoUser { get; set; }

    public string? DemoPassword { get; set; }
}
