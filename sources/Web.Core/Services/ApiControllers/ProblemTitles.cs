namespace Web.Core.Services.ApiControllers;

/// <summary>
/// Fixed ProblemDetails titles the web client tells apart when the status code alone is ambiguous
/// (mirrored in <c>Web.Client/src/lib/api/problemTitles.ts</c>).
/// </summary>
public static class ProblemTitles
{
    /// <summary>403 at login: the password was right, the address is not confirmed yet.</summary>
    public const string EmailNotConfirmed = "E-mail not confirmed";

    /// <summary>403 at login: the password was right, but the adult belongs to no family (any more) (LP-107).</summary>
    public const string NoMembership = "No membership";
}
