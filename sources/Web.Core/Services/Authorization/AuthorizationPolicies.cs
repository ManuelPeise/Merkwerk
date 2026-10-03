using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Web.Core.Services.Authorization;

/// <summary>
/// Default policy: signed in and no pending password change. While a start password is active only endpoints with
/// <see cref="PasswordChangeAllowed"/> work (me, change-password); everything else answers 403 (LP-104).
/// </summary>
public static class AuthorizationPolicies
{
    public const string PasswordChangeAllowed = nameof(PasswordChangeAllowed);

    public static void Configure(AuthorizationOptions options)
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(context => !context.User.HasClaim(AuthClaims.MustChangePassword, "true"))
            .Build();

        options.AddPolicy(PasswordChangeAllowed, policy => policy.RequireAuthenticatedUser());
    }
}
