using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Web.Core.Services.Authorization;

/// <summary>
/// Default policy: signed in and no pending password change. While a start password is active only endpoints with
/// <see cref="PasswordChangeAllowed"/> work (me, change-password); everything else answers 403 (LP-104).
/// <see cref="Member"/> and <see cref="OrgAdmin"/> add the role from the membership (LP-105; complete set in LP-107).
/// Never use <c>[Authorize(Roles = …)]</c>: it skips the default policy and with it the start-password check.
/// </summary>
public static class AuthorizationPolicies
{
    public const string PasswordChangeAllowed = nameof(PasswordChangeAllowed);

    /// <summary>Any adult of the family.</summary>
    public const string Member = nameof(Member);

    /// <summary>Admins of the family (owner included).</summary>
    public const string OrgAdmin = nameof(OrgAdmin);

    public static void Configure(AuthorizationOptions options)
    {
        options.DefaultPolicy = Signedin().Build();

        options.AddPolicy(PasswordChangeAllowed, policy => policy.RequireAuthenticatedUser());
        options.AddPolicy(Member, Signedin().RequireRole(AuthRoles.Member, AuthRoles.OrgAdmin).Build());
        options.AddPolicy(OrgAdmin, Signedin().RequireRole(AuthRoles.OrgAdmin).Build());
    }

    private static AuthorizationPolicyBuilder Signedin() => new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireAssertion(context => !context.User.HasClaim(AuthClaims.MustChangePassword, "true"));
}
