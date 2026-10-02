using System.Text;
using System.Text.Json;

namespace Architecture.Tests;

/// <summary>
/// Turns shared/design-tokens/tokens.json (DTCG) into CSS custom properties: color.brand.primary → --color-brand-primary.
/// Kept in the test project on purpose: no Node/Style Dictionary needed, and CI fails when tokens.css is out of date.
/// </summary>
internal static class DesignTokenCss
{
    public const string Header =
        "/* Generated from shared/design-tokens/tokens.json - do not edit. Regenerate: see shared/design-tokens/README.md */";

    public static string Generate(string tokensJson)
    {
        using var document = JsonDocument.Parse(tokensJson);
        var css = new StringBuilder();
        css.Append(Header).Append('\n');
        css.Append(":root {\n");
        foreach (var (name, value) in Flatten(document.RootElement))
        {
            css.Append("  --").Append(name).Append(": ").Append(value).Append(";\n");
        }

        css.Append("}\n");
        return css.ToString();
    }

    /// <summary>All tokens as (css name without "--", css value), in file order.</summary>
    public static IReadOnlyList<(string Name, string Value)> Flatten(JsonElement root)
    {
        var result = new List<(string, string)>();
        Walk(root, [], result);
        return result;
    }

    private static void Walk(JsonElement node, List<string> path, List<(string, string)> result)
    {
        if (node.TryGetProperty("$value", out var value))
        {
            result.Add((string.Join('-', path), Format(value)));
            return;
        }

        foreach (var property in node.EnumerateObject())
        {
            if (property.Name.StartsWith('$') || property.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            path.Add(Kebab(property.Name));
            Walk(property.Value, path, result);
            path.RemoveAt(path.Count - 1);
        }
    }

    private static string Format(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Array => string.Join(", ", value.EnumerateArray().Select(item => QuoteIfNeeded(item.GetString() ?? string.Empty))),
        JsonValueKind.String => value.GetString() ?? string.Empty,
        _ => value.GetRawText(),
    };

    private static string QuoteIfNeeded(string family) => family.Contains(' ', StringComparison.Ordinal) ? $"\"{family}\"" : family;

    private static string Kebab(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(name[i]));
        }

        return builder.ToString();
    }
}
