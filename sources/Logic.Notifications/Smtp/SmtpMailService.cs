using Logic.Notifications.Rendering;
using Logic.Shared.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Enums;
using Shared.Models.Notifications;

namespace Logic.Notifications.Smtp;

/// <summary>
/// Sends mails with MailKit, one connection per mail (LP-162). Logs template, language and outcome –
/// never the recipient's address.
/// </summary>
internal sealed partial class SmtpMailService : IMailService
{
    private readonly MailTemplateRenderer _renderer;
    private readonly IOptions<MailOptions> _options;
    private readonly ILogger<SmtpMailService> _logger;

    public SmtpMailService(
        MailTemplateRenderer renderer,
        IOptions<MailOptions> options,
        ILogger<SmtpMailService> logger)
    {
        _renderer = renderer;
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(MailMessageRequest request, CancellationToken cancellationToken)
    {
        var settings = _options.Value;
        var rendered = _renderer.Render(request.Template, request.Language, request.Values);
        using var message = CreateMessage(settings, request, rendered);

        try
        {
            using var client = new SmtpClient { Timeout = settings.TimeoutSeconds * 1000 };
            await client.ConnectAsync(settings.Host, settings.Port, ToSocketOptions(settings.Security), cancellationToken);

            if (!string.IsNullOrEmpty(settings.UserName))
            {
                await client.AuthenticateAsync(settings.UserName, settings.Password ?? string.Empty, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogFailed(request.Template, request.Language, exception.GetType().Name);
            throw new MailDeliveryException(request.Template, exception);
        }

        LogSent(request.Template, request.Language);
    }

    private static MimeMessage CreateMessage(MailOptions settings, MailMessageRequest request, RenderedMail rendered)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(new MailboxAddress(request.ToName, request.ToAddress));
        message.Subject = rendered.Subject;
        message.Body = new BodyBuilder { HtmlBody = rendered.HtmlBody, TextBody = rendered.TextBody }.ToMessageBody();
        return message;
    }

    private static SecureSocketOptions ToSocketOptions(MailSecurity security) => security switch
    {
        MailSecurity.None => SecureSocketOptions.None,
        MailSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
        _ => SecureSocketOptions.StartTls,
    };

    [LoggerMessage(Level = LogLevel.Information, Message = "Mail {Template} ({Language}) sent.")]
    private partial void LogSent(MailTemplate template, string language);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Mail {Template} ({Language}) could not be delivered: {Error}.")]
    private partial void LogFailed(MailTemplate template, string language, string error);
}
