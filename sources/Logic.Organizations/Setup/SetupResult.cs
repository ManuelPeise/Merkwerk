using Logic.Authentication;

namespace Logic.Organizations.Setup;

/// <summary><see cref="Errors"/> maps a field name (camelCase, like the request) to its messages.</summary>
public sealed record SetupResult(
    SetupStatus Status,
    IReadOnlyDictionary<string, string[]> Errors,
    AuthSession? Session = null);
