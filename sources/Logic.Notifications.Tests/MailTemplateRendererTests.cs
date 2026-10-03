using Logic.Notifications.Rendering;
using Shared.Enums;

namespace Logic.Notifications.Tests;

public sealed class MailTemplateRendererTests
{
    private readonly MailTemplateRenderer _renderer = new();

    [Theory]
    [MemberData(nameof(TemplateValues.AllTemplatesAndLanguages), MemberType = typeof(TemplateValues))]
    public void Render_AllValues_FillsSubjectHtmlAndText(MailTemplate template, string language)
    {
        var values = TemplateValues.For(template);

        var mail = _renderer.Render(template, language, values);

        Assert.False(string.IsNullOrWhiteSpace(mail.Subject));
        Assert.DoesNotContain("{{", mail.HtmlBody);
        Assert.DoesNotContain("{{", mail.TextBody);
        Assert.Contains($"lang=\"{language}\"", mail.HtmlBody);
        Assert.All(values.Values, value => Assert.Contains(value, mail.TextBody));
    }

    [Theory]
    [MemberData(nameof(TemplateValues.AllTemplatesAndLanguages), MemberType = typeof(TemplateValues))]
    public void Render_ValueMissing_Throws(MailTemplate template, string language)
    {
        var values = TemplateValues.For(template);
        values.Remove("Name");

        var exception = Assert.Throws<InvalidOperationException>(() => _renderer.Render(template, language, values));

        Assert.Contains("'Name'", exception.Message);
    }

    [Fact]
    public void Render_ValueWithHtml_IsEscapedInHtmlAndRawInText()
    {
        var values = TemplateValues.For(MailTemplate.ConfirmEmail);
        values["Name"] = "<b>Tom & Jerry</b>";

        var mail = _renderer.Render(MailTemplate.ConfirmEmail, "en", values);

        Assert.Contains("&lt;b&gt;Tom &amp; Jerry&lt;/b&gt;", mail.HtmlBody);
        Assert.DoesNotContain("<b>Tom", mail.HtmlBody);
        Assert.Contains("<b>Tom & Jerry</b>", mail.TextBody);
    }

    [Fact]
    public void Render_UnsupportedLanguage_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _renderer.Render(MailTemplate.ConfirmEmail, "fr", TemplateValues.For(MailTemplate.ConfirmEmail)));
    }

    [Fact]
    public void Render_GermanTemplate_SubjectIsPlainText()
    {
        var mail = _renderer.Render(MailTemplate.ConfirmEmail, "de", TemplateValues.For(MailTemplate.ConfirmEmail));

        Assert.Equal("Bitte bestätige deine E-Mail-Adresse", mail.Subject);
    }
}
