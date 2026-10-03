using Shared.Enums;

namespace Logic.Notifications;

/// <summary>The mail could not be delivered to the SMTP server. The message never contains the recipient's address.</summary>
public sealed class MailDeliveryException(MailTemplate template, Exception innerException)
    : Exception($"Mail '{template}' could not be delivered.", innerException)
{
    public MailTemplate Template { get; } = template;
}
