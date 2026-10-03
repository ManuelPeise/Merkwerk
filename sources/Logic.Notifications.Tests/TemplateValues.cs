namespace Logic.Notifications.Tests;

/// <summary>A complete set of values for every template, plus helpers to build test data.</summary>
internal static class TemplateValues
{
    public static readonly IReadOnlyDictionary<MailTemplate, string[]> Required = new Dictionary<MailTemplate, string[]>
    {
        [MailTemplate.ConfirmEmail] = ["Name", "Link"],
        [MailTemplate.Invitation] = ["Name", "InvitedBy", "OrganizationName", "Link", "ExpiresAt"],
        [MailTemplate.PasswordReset] = ["Name", "Link", "ExpiresAt"],
        [MailTemplate.OneTimeCode] = ["Name", "Code", "ExpiresAt"],
    };

    public static IEnumerable<object[]> AllTemplatesAndLanguages() =>
        from template in Enum.GetValues<MailTemplate>()
        from language in new[] { "de", "en" }
        select new object[] { template, language };

    public static Dictionary<string, string> For(MailTemplate template, string marker = "") =>
        Required[template].ToDictionary(name => name, name => name switch
        {
            "Link" => "https://merkwerk.example/action?token=abc123",
            "Code" => "K7Q-4M2-X9P",
            "ExpiresAt" => "05.10.2026 18:00",
            "Name" => "Anna" + marker,
            _ => name + "-value",
        });
}
