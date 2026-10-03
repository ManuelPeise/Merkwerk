using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Web.Core.Services.Authorization;

/// <summary>
/// Default policy: a signed-in adult without a pending password change. While a start password is active only endpoints
/// with <see cref="PasswordChangeAllowed"/> work (change-password); everything else answers 403 (LP-104).
/// <see cref="Member"/> and <see cref="OrgAdmin"/> add the role from the membership (LP-105; complete set in LP-107).
/// Children's tokens (role <see cref="AuthRoles.Learner"/>, LP-106) pass only <see cref="Learner"/> and
/// <see cref="AnySession"/> – their <c>sub</c> is a learner id, never a user id.
/// Never use <c>[Authorize(Roles = …)]</c>: it skips the default policy and with it the start-password check.
/// </summary>
public static class AuthorizationPolicies
{
    public const string PasswordChangeAllowed = nameof(PasswordChangeAllowed);

    /// <summary>Any adult of the family.</summary>
    public const string Member = nameof(Member);

    /// <summary>Admins of the family (owner included).</summary>
    public const string OrgAdmin = nameof(OrgAdmin);

    /// <summary>A child signed in on a paired device (LP-106).</summary>
    public const string Learner = nameof(Learner);

    /// <summary>Any session – adult (also with a start password) or child. Only for "who am I" (me).</summary>
    public const string AnySession = nameof(AnySession);

    public static void Configure(AuthorizationOptions options)
    {
        options.DefaultPolicy = SignedInAdult().Build();

        options.AddPolicy(PasswordChangeAllowed, Adult().Build());
        options.AddPolicy(Member, SignedInAdult().RequireRole(AuthRoles.Member, AuthRoles.OrgAdmin).Build());
        options.AddPolicy(OrgAdmin, SignedInAdult().RequireRole(AuthRoles.OrgAdmin).Build());
        options.AddPolicy(Learner, policy => policy.RequireAuthenticatedUser().RequireRole(AuthRoles.Learner));
        options.AddPolicy(AnySession, policy => policy.RequireAuthenticatedUser());
    }

    private static AuthorizationPolicyBuilder Adult() => new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireAssertion(context => !context.User.IsInRole(AuthRoles.Learner));

    private static AuthorizationPolicyBuilder SignedInAdult() => Adult()
        .RequireAssertion(context => !context.User.HasClaim(AuthClaims.MustChangePassword, "true"));
}
