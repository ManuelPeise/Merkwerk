using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Logic.Notifications.Rendering;

/// <summary>
/// Fills the embedded templates <c>Templates/{language}/{Template}.html|.txt</c>. Placeholders look like <c>{{Name}}</c>;
/// values are HTML-escaped in the HTML part. A placeholder without a value is an error, never an empty gap.
/// The subject is the HTML <c>&lt;title&gt;</c>.
/// </summary>
internal sealed partial class MailTemplateRenderer
{
    public static readonly IReadOnlyList<string> SupportedLanguages = ["de", "en"];

    private static readonly Assembly ResourceAssembly = typeof(MailTemplateRenderer).Assembly;

    public RenderedMail Render(MailTemplate template, string language, IReadOnlyDictionary<string, string> values)
    {
        if (!SupportedLanguages.Contains(language))
        {
            throw new ArgumentException($"Language '{language}' is not supported (de, en).", nameof(language));
        }

        var html = Fill(ReadTemplate(template, language, "html"), values, WebUtility.HtmlEncode, template);
        var text = Fill(ReadTemplate(template, language, "txt"), values, value => value, template);

        var title = TitlePattern().Match(html);
        if (!title.Success)
        {
            throw new InvalidOperationException($"Template {language}/{template}.html has no <title> (used as subject).");
        }

        // The title is already HTML-encoded; the subject header needs plain text.
        return new RenderedMail(WebUtility.HtmlDecode(title.Groups[1].Value.Trim()), html, text);
    }

    private static string Fill(
        string content,
        IReadOnlyDictionary<string, string> values,
        Func<string, string> encode,
        MailTemplate template) =>
        PlaceholderPattern().Replace(content, match =>
        {
            var name = match.Groups[1].Value;
            return values.TryGetValue(name, out var value)
                ? encode(value)
                : throw new InvalidOperationException($"Mail template {template} needs a value for '{name}'.");
        });

    private static string ReadTemplate(MailTemplate template, string language, string extension)
    {
        var resourceName = $"Logic.Notifications.Templates.{language}.{template}.{extension}";
        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Mail template resource '{resourceName}' is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderPattern();

    [GeneratedRegex(@"<title>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TitlePattern();
}
