using Data.Database.Abstractions;
using Logic.Authentication;

namespace Web.Core.Services.CurrentUser;

/// <summary>
/// <see cref="ICurrentUser"/> from the access token of the current request (claims are not remapped, MapInboundClaims = false).
/// Anonymous requests act as <c>system</c> without an organization.
/// </summary>
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Actor
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var subject = user?.FindFirst(AuthClaims.Subject)?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                return SystemCurrentUser.SystemActor;
            }

            return user!.IsInRole(AuthRoles.Learner) ? $"learner:{subject}" : $"user:{subject}";
        }
    }

    public long? OrganizationId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(AuthClaims.OrganizationId)?.Value;
            return long.TryParse(value, out var organizationId) ? organizationId : null;
        }
    }
}
