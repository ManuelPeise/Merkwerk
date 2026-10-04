using System.Globalization;
using System.Text.Json;
using Logic.Content.Subjects;

namespace Architecture.Tests;

/// <summary>
/// LP-109: every subject color of the fixed choice exists as a design token and carries white text and icons with
/// at least 4.5:1 (WCAG AA). The web client's theme mirrors the same values (Web.Client/src/lib/theme/theme.ts).
/// </summary>
public sealed class SubjectPaletteTests
{
    private const double MinimumContrast = 4.5;

    [Fact]
    public void SubjectColors_ExistAsDesignTokens_WithContrastForWhite()
    {
        var tokens = LoadSubjectTokens();

        var problems = SubjectRules.Colors
            .Select(key => key["subject.".Length..])
            .Select(name => tokens.TryGetValue(name, out var hex)
                ? ContrastWithWhite(hex) >= MinimumContrast ? null : $"{name}: {hex} has only {ContrastWithWhite(hex):0.00}:1"
                : $"{name}: missing in tokens.json")
            .Where(problem => problem is not null)
            .ToList();

        Assert.Empty(problems);
    }

    [Fact]
    public void SubjectChoice_HasNoDuplicates()
    {
        Assert.Equal(SubjectRules.Colors.Count, SubjectRules.Colors.Distinct().Count());
        Assert.Equal(SubjectRules.Icons.Count, SubjectRules.Icons.Distinct().Count());
        Assert.Equal(SubjectRules.LanguageCodes.Count, SubjectRules.LanguageCodes.Distinct().Count());
        Assert.All(SubjectRules.Colors, key => Assert.StartsWith("subject.", key, StringComparison.Ordinal));
    }

    private static Dictionary<string, string> LoadSubjectTokens()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "DesignTokens", "tokens.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return document.RootElement.GetProperty("color").GetProperty("subject").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetProperty("$value").GetString()!);
    }

    private static double ContrastWithWhite(string hex) => 1.05 / (RelativeLuminance(hex) + 0.05);

    private static double RelativeLuminance(string hex)
    {
        var channels = Enumerable.Range(0, 3)
            .Select(i => int.Parse(hex.AsSpan(1 + (i * 2), 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0)
            .Select(c => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4))
            .ToArray();

        return (0.2126 * channels[0]) + (0.7152 * channels[1]) + (0.0722 * channels[2]);
    }
}
