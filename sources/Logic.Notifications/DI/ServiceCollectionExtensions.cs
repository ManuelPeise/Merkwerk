using Logic.Notifications.Formatting;
using Logic.Notifications.Links;
using Logic.Notifications.Rendering;
using Logic.Notifications.Smtp;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.Notifications.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IMailService"/> (SMTP via MailKit), <see cref="IPublicLinkBuilder"/> and <see cref="IMailDateFormatter"/>.</summary>
    public static IServiceCollection AddMerkwerkNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MailOptions>()
            .Bind(configuration.GetSection(MailOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host),
                "Mail:Host is missing. Development: start Mailpit (deploy/setup-local.ps1), production: MAIL_HOST in deploy/.env.")
            .Validate(o => o.Port is > 0 and <= 65535, "Mail:Port must be between 1 and 65535.")
            .Validate(o => o.FromAddress.Contains('@'), "Mail:FromAddress must be an e-mail address.")
            .Validate(o => o.TimeoutSeconds > 0, "Mail:TimeoutSeconds must be positive.")
            .ValidateOnStart();

        services.AddOptions<PublicUrlOptions>()
            .Bind(configuration.GetSection(PublicUrlOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.PublicBaseUrl, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp),
                "App:PublicBaseUrl must be an absolute http(s) URL, e.g. https://merkwerk.example.")
            .ValidateOnStart();

        services.AddSingleton<MailTemplateRenderer>();
        services.AddSingleton<IMailService, SmtpMailService>();
        services.AddSingleton<IPublicLinkBuilder, PublicLinkBuilder>();
        services.AddSingleton<IMailDateFormatter, MailDateFormatter>();

        return services;
    }
}
