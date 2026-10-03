namespace Logic.Notifications;

/// <summary>SMTP settings (section "Mail"). Development: Mailpit on localhost:1025 without TLS.</summary>
public sealed class MailOptions
{
    public const string SectionName = "Mail";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public MailSecurity Security { get; set; } = MailSecurity.StartTls;

    /// <summary>Empty = no authentication (Mailpit). Comes from user secrets / environment, never from appsettings.</summary>
    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Merkwerk";

    /// <summary>Connect + send timeout.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}

public enum MailSecurity
{
    None,
    StartTls,
    SslOnConnect,
}
