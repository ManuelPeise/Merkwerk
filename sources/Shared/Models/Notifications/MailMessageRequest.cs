using Shared.Enums;

namespace Shared.Models.Notifications;

/// <summary>
/// One mail to send. <paramref name="Values"/> fill the placeholders of the template (e.g. <c>Name</c>, <c>Link</c>,
/// <c>Code</c>, <c>ExpiresAt</c>); <paramref name="Language"/> is <c>de</c> or <c>en</c>.
/// </summary>
public sealed record MailMessageRequest(
    string ToAddress,
    string ToName,
    MailTemplate Template,
    string Language,
    IReadOnlyDictionary<string, string> Values);
