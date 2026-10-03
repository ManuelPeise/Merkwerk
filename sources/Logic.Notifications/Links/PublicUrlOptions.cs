namespace Logic.Notifications.Links;

/// <summary>Public address and local time zone of this instance (section "App"), used for links and dates in mails.</summary>
public sealed class PublicUrlOptions
{
    public const string SectionName = "App";

    /// <summary>e.g. <c>https://merkwerk.familie.de</c>; development: <c>http://localhost:65350</c>.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>IANA time zone for dates in mails (all times are stored as UTC).</summary>
    public string TimeZoneId { get; set; } = "Europe/Berlin";
}
