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

    /// <summary>Organization of the session (org_id claim, LP-105), or null without membership.</summary>
    protected long? CurrentOrganizationId =>
        long.TryParse(User.FindFirst(AuthClaims.OrganizationId)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;

    /// <summary>
    /// 400 with messages per field (camelCase keys, like the automatic model validation) – for rule errors the
    /// services report after the data annotations passed.
    /// </summary>
    protected ActionResult FieldErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
        {
            foreach (var message in messages)
            {
                ModelState.AddModelError(field, message);
            }
        }

        return ValidationProblem(ModelState);
    }

    /// <summary>
    /// Id of the signed-in adult (sub claim), or null for anonymous calls and children – a child's sub is a learner id
    /// and must never be taken for a user id (LP-106).
    /// </summary>
    protected long? CurrentUserId =>
        !User.IsInRole(AuthRoles.Learner)
        && long.TryParse(User.FindFirst(AuthClaims.Subject)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;
}
