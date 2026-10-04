using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Logic.Notifications.Rendering;
using Logic.Notifications.Smtp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shared.Enums;
using Shared.Models.Notifications;

namespace Logic.Notifications.Tests;

/// <summary>LP-162: real SMTP delivery into Mailpit (needs Docker), checked through the Mailpit API.</summary>
public sealed class MailpitTests : IAsyncLifetime
{
    private const ushort SmtpPort = 1025;
    private const ushort ApiPort = 8025;

    private readonly IContainer _mailpit = new ContainerBuilder("axllent/mailpit:latest")
        .WithPortBinding(SmtpPort, true)
        .WithPortBinding(ApiPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request.ForPort(ApiPort).ForPath("/api/v1/messages")))
        .Build();

    private HttpClient _api = null!;

    public async Task InitializeAsync()
    {
        await _mailpit.StartAsync();
        _api = new HttpClient
        {
            BaseAddress = new UriBuilder("http", _mailpit.Hostname, _mailpit.GetMappedPublicPort(ApiPort)).Uri,
        };
    }

    public async Task DisposeAsync()
    {
        _api.Dispose();
        await _mailpit.DisposeAsync();
    }

    [Fact]
    public async Task SendAsync_EveryTemplateAndLanguage_ArrivesWithSubjectHtmlAndText()
    {
        // Arrange
        var service = CreateService(_mailpit.Hostname, _mailpit.GetMappedPublicPort(SmtpPort));
        var renderer = new MailTemplateRenderer();
        var expectedSubjects = new List<string>();

        // Act
        foreach (var data in TemplateValues.AllTemplatesAndLanguages())
        {
            var (template, language) = ((MailTemplate)data[0], (string)data[1]);
            var values = TemplateValues.For(template);
            expectedSubjects.Add(renderer.Render(template, language, values).Subject);
            await service.SendAsync(
                new MailMessageRequest("anna@example.org", "Anna", template, language, values),
                CancellationToken.None);
        }

        // Assert
        var list = await _api.GetFromJsonAsync<MessageList>("/api/v1/messages");
        Assert.NotNull(list);
        Assert.Equal(expectedSubjects.Order(), list.Messages.Select(m => m.Subject).Order());
        Assert.All(list.Messages, m => Assert.Equal("anna@example.org", Assert.Single(m.To).Address));

        var codeSubject = renderer.Render(MailTemplate.OneTimeCode, "de", TemplateValues.For(MailTemplate.OneTimeCode)).Subject;
        var code = list.Messages.Single(m => m.Subject == codeSubject);
        var detail = await _api.GetFromJsonAsync<MessageDetail>($"/api/v1/message/{code.Id}");
        Assert.NotNull(detail);
        Assert.Contains("K7Q-4M2-X9P", detail.Html);
        Assert.Contains("K7Q-4M2-X9P", detail.Text);
    }

    internal static SmtpMailService CreateService(string host, int port, int timeoutSeconds = 15) => new(
        new MailTemplateRenderer(),
        Options.Create(new MailOptions
        {
            Host = host,
            Port = port,
            Security = MailSecurity.None,
            FromAddress = "noreply@merkwerk.local",
            FromName = "Merkwerk",
            TimeoutSeconds = timeoutSeconds,
        }),
        NullLogger<SmtpMailService>.Instance);

    private sealed record MessageList([property: JsonPropertyName("messages")] IReadOnlyList<MessageSummary> Messages);

    private sealed record MessageSummary(
        [property: JsonPropertyName("ID")] string Id,
        [property: JsonPropertyName("Subject")] string Subject,
        [property: JsonPropertyName("To")] IReadOnlyList<MailAddress> To);

    private sealed record MailAddress([property: JsonPropertyName("Address")] string Address);

    private sealed record MessageDetail(
        [property: JsonPropertyName("HTML")] string Html,
        [property: JsonPropertyName("Text")] string Text);
}
