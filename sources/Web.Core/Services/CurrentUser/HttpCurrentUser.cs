using Data.Database.Abstractions;

namespace Web.Core.Services.CurrentUser;

/// <summary>
/// <see cref="ICurrentUser"/> from the access token of the current request (claims are not remapped, MapInboundClaims = false).
/// Anonymous requests act as <c>system</c> without an organization.
/// </summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string LearnerRole = "Learner";

    /// <summary>Organization claim in the access token – issued by TokenService from LP-104/LP-107 on.</summary>
    public const string OrganizationIdClaim = "org_id";

    public string Actor
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var subject = user?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                return SystemCurrentUser.SystemActor;
            }

            return user!.IsInRole(LearnerRole) ? $"learner:{subject}" : $"user:{subject}";
        }
    }

    public long? OrganizationId
    {
        get
        {
            // Claim is issued with LP-104/LP-107; until then every request has no organization.
            var value = httpContextAccessor.HttpContext?.User.FindFirst(OrganizationIdClaim)?.Value;
            return long.TryParse(value, out var organizationId) ? organizationId : null;
        }
    }
}
