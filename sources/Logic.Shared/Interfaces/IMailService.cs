using Shared.Models.Notifications;

namespace Logic.Shared.Interfaces;

/// <summary>Sends a templated e-mail to an adult (never data about children – LP-162).</summary>
public interface IMailService
{
    /// <summary>Renders and sends the mail. Throws <c>MailDeliveryException</c> if the SMTP server refuses or is unreachable.</summary>
    Task SendAsync(MailMessageRequest request, CancellationToken cancellationToken);
}
