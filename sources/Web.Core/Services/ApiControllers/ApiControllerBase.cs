using System.Globalization;
using Logic.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Web.Core.Services.ApiControllers;

/// <summary>
/// Base for all API controllers: route <c>/api/v1/{controller}/{action}</c> (lowercase, kebab-case – see
/// ServiceRegistrationExtensions), automatic model validation and ProblemDetails for 4xx results.
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]
public abstract class ApiControllerBase : ControllerBase
{
    private static readonly string[] MailLanguages = ["de", "en"];

    /// <summary>Language for mails: the first supported one from Accept-Language, otherwise German.</summary>
    protected string MailLanguage =>
        Request.GetTypedHeaders().AcceptLanguage
            .OrderByDescending(l => l.Quality ?? 1)
            .Select(l => l.Value.Value?[..Math.Min(2, l.Value.Length)].ToLowerInvariant())
            .FirstOrDefault(l => l is not null && MailLanguages.Contains(l))
        ?? "de";

    /// <summary>Id of the signed-in adult (sub claim), or null for anonymous calls.</summary>
    protected long? CurrentUserId =>
        long.TryParse(User.FindFirst(AuthClaims.Subject)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;
}
