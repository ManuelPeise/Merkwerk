using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Logic.Content.Grading;

/// <summary>Tolerance rules for typed answers (LP-111), shared by text and cloze questions.</summary>
internal static partial class AnswerText
{
    private static readonly char[] EndPunctuation = ['.', '!', '?'];

    /// <summary>
    /// Trims, joins inner whitespace to one space, unifies Unicode forms (composed umlauts), drops end punctuation and,
    /// unless case-sensitive, lower-cases.
    /// </summary>
    public static string Normalize(string? text, bool caseSensitive)
    {
        var value = Whitespace().Replace((text ?? string.Empty).Normalize(NormalizationForm.FormC), " ").Trim();
        value = value.TrimEnd(EndPunctuation).TrimEnd();

        return caseSensitive ? value : value.ToLowerInvariant();
    }

    /// <summary>
    /// Reads a number with a dot or comma as decimal separator ("0,5" = "0.5"). Thousands separators are not supported
    /// (grades 1–4 type plain numbers).
    /// </summary>
    public static bool TryParseNumber(string? text, out decimal value) =>
        decimal.TryParse(
            Whitespace().Replace(text ?? string.Empty, string.Empty).Replace(',', '.'),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);

    /// <summary>
    /// Exactly one typo: one letter missing, added, replaced, or two neighbours swapped ("cta" for "cat").
    /// </summary>
    public static bool IsOneTypoAway(string expected, string actual) => TypoDistance(expected, actual) == 1;

    /// <summary>Optimal string alignment distance (Levenshtein plus swapped neighbours).</summary>
    private static int TypoDistance(string a, string b)
    {
        if (Math.Abs(a.Length - b.Length) > 1)
        {
            return 2;
        }

        var d = new int[a.Length + 1, b.Length + 1];

        for (var i = 0; i <= a.Length; i++)
        {
            d[i, 0] = i;
        }

        for (var j = 0; j <= b.Length; j++)
        {
            d[0, j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);

                if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                {
                    d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + 1);
                }
            }
        }

        return d[a.Length, b.Length];
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
