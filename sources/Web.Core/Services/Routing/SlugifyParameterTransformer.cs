using System.Text.RegularExpressions;

namespace Web.Core.Services.Routing;

/// <summary>Route tokens in kebab-case: ForgotPassword → forgot-password (LP-104).</summary>
public sealed partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) =>
        value is null ? null : WordBoundary().Replace(value.ToString()!, "$1-$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundary();
}
