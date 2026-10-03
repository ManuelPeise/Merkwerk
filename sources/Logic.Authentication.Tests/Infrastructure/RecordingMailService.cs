using Logic.Notifications;

namespace Logic.Authentication.Tests.Infrastructure;

/// <summary>Keeps every mail instead of sending it, so tests can follow the links.</summary>
public sealed class RecordingMailService : IMailService
{
    private readonly List<MailMessageRequest> _sent = [];

    public IReadOnlyList<MailMessageRequest> Sent => _sent;

    public Task SendAsync(MailMessageRequest request, CancellationToken cancellationToken)
    {
        _sent.Add(request);
        return Task.CompletedTask;
    }

    public MailMessageRequest Last(MailTemplate template) => _sent.Last(m => m.Template == template);

    /// <summary>Query value of the mail's link, e.g. "token".</summary>
    public string LinkValue(MailTemplate template, string name)
    {
        var query = new Uri(Last(template).Values["Link"]).Query.TrimStart('?').Split('&');
        var pair = query.Select(p => p.Split('=', 2)).Single(p => p[0] == name);
        return Uri.UnescapeDataString(pair[1]);
    }
}
