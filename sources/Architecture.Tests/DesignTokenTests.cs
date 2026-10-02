using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Architecture.Tests;

/// <summary>
/// LP-008: tokens.css must match tokens.json, and the colour pairs we rely on must meet WCAG AA.
/// Regenerate tokens.css after changing tokens.json (PowerShell, from the repo root):
///   $env:MERKWERK_UPDATE_TOKENS = "1"; dotnet test sources/Merkwerk.slnx --filter DesignTokenTests; Remove-Item Env:MERKWERK_UPDATE_TOKENS
/// </summary>
public sealed class DesignTokenTests
{
    private const string UpdateVariable = "MERKWERK_UPDATE_TOKENS";

    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string TokensJsonPath = Path.Combine(RepositoryRoot, "shared", "design-tokens", "tokens.json");
    private static readonly string TokensCssPath = Path.Combine(RepositoryRoot, "sources", "Web.Client", "wwwroot", "css", "tokens.css");

    [Fact]
    public void TokensCss_IsUpToDate()
    {
        var expected = DesignTokenCss.Generate(File.ReadAllText(TokensJsonPath));

        if (Environment.GetEnvironmentVariable(UpdateVariable) == "1")
        {
            File.WriteAllText(TokensCssPath, expected, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        var actual = File.Exists(TokensCssPath) ? File.ReadAllText(TokensCssPath) : string.Empty;

        Assert.True(
            Normalize(actual) == Normalize(expected),
            $"tokens.css is out of date. Regenerate it with {UpdateVariable}=1 (see shared/design-tokens/README.md).");
    }

    /// <summary>Text needs 4.5:1, large text (≥ 24px) and UI parts 3:1 (WCAG 2.2 AA, 1.4.3 and 1.4.11).</summary>
    [Theory]
    [InlineData("color-text-primary", "color-surface-background", 4.5)]
    [InlineData("color-text-primary", "color-surface-card", 4.5)]
    [InlineData("color-text-secondary", "color-surface-background", 4.5)]
    [InlineData("color-text-secondary", "color-surface-card", 4.5)]
    [InlineData("color-text-on-primary", "color-brand-primary", 4.5)]
    [InlineData("color-brand-primary", "color-surface-background", 4.5)]
    [InlineData("color-brand-primary-strong", "color-brand-primary-soft", 4.5)]
    [InlineData("color-text-on-primary", "color-subject-german", 4.5)]
    [InlineData("color-text-on-primary", "color-subject-english", 4.5)]
    [InlineData("color-text-on-primary", "color-subject-math", 4.5)]
    [InlineData("color-feedback-correct", "color-feedback-correct-bg", 4.5)]
    [InlineData("color-feedback-retry", "color-feedback-retry-bg", 4.5)]
    [InlineData("color-part-of-speech-noun", "color-surface-card", 4.5)]
    [InlineData("color-part-of-speech-verb", "color-surface-card", 4.5)]
    [InlineData("color-part-of-speech-adjective", "color-surface-card", 4.5)]
    [InlineData("color-border-strong", "color-surface-card", 3.0)]
    public void ColourPair_MeetsWcagAa(string foreground, string background, double minimum)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(TokensJsonPath));
        var colours = DesignTokenCss.Flatten(document.RootElement).ToDictionary(t => t.Name, t => t.Value);

        var ratio = ContrastRatio(colours[foreground], colours[background]);

        Assert.True(ratio >= minimum, $"{foreground} on {background}: {ratio:0.00}:1, needs {minimum}:1");
    }

    private static double ContrastRatio(string a, string b)
    {
        var first = Luminance(a);
        var second = Luminance(b);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static double Luminance(string hex)
    {
        static double Channel(string hex, int start)
        {
            var c = int.Parse(hex.AsSpan(start, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(hex, 1)) + (0.7152 * Channel(hex, 3)) + (0.0722 * Channel(hex, 5));
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd();

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "shared", "design-tokens", "tokens.json")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException($"shared/design-tokens/tokens.json not found above {AppContext.BaseDirectory}.");
    }
}
