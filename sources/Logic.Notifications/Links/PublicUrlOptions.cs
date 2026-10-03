namespace Logic.Notifications.Links;

/// <summary>Public address of this instance (section "App"), the base of every link in a mail.</summary>
public sealed class PublicUrlOptions
{
    public const string SectionName = "App";

    /// <summary>e.g. <c>https://merkwerk.familie.de</c>; development: <c>http://localhost:65350</c>.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}
