using System.Security.Claims;
using Logic.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Web.Core.Services.Authorization;

namespace Architecture.Tests;

/// <summary>
/// LP-107: who passes which policy. The principals look like the access tokens TokenService issues (claims are not
/// remapped, role claim type "role").
/// </summary>
public sealed class AuthorizationPolicyTests
{
    private static readonly ServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddAuthorization(AuthorizationPolicies.Configure)
        .BuildServiceProvider();

    private static readonly IAuthorizationService Authorization = Services.GetRequiredService<IAuthorizationService>();

    private static readonly IAuthorizationPolicyProvider PolicyProvider = Services.GetRequiredService<IAuthorizationPolicyProvider>();

    public static TheoryData<string, string, bool> Matrix => new()
    {
        // Policy                                     Session                Allowed
        { AuthorizationPolicies.Member,               "member",              true },
        { AuthorizationPolicies.Member,               "orgAdmin",            true },
        { AuthorizationPolicies.Member,               "learner",             false },
        { AuthorizationPolicies.Member,               "startPassword",       false },
        { AuthorizationPolicies.Member,               "noRole",              false },
        { AuthorizationPolicies.Member,               "anonymous",           false },

        { AuthorizationPolicies.OrgAdmin,             "orgAdmin",            true },
        { AuthorizationPolicies.OrgAdmin,             "member",              false },
        { AuthorizationPolicies.OrgAdmin,             "learner",             false },
        { AuthorizationPolicies.OrgAdmin,             "startPasswordAdmin",  false },
        { AuthorizationPolicies.OrgAdmin,             "noRole",              false },
        { AuthorizationPolicies.OrgAdmin,             "anonymous",           false },

        { AuthorizationPolicies.Learner,              "learner",             true },
        { AuthorizationPolicies.Learner,              "member",              false },
        { AuthorizationPolicies.Learner,              "orgAdmin",            false },
        { AuthorizationPolicies.Learner,              "anonymous",           false },

        { AuthorizationPolicies.PasswordChangeAllowed, "member",             true },
        { AuthorizationPolicies.PasswordChangeAllowed, "startPassword",      true },
        { AuthorizationPolicies.PasswordChangeAllowed, "learner",            false },
        { AuthorizationPolicies.PasswordChangeAllowed, "anonymous",          false },

        { AuthorizationPolicies.AnySession,           "member",              true },
        { AuthorizationPolicies.AnySession,           "startPassword",       true },
        { AuthorizationPolicies.AnySession,           "learner",             true },
        { AuthorizationPolicies.AnySession,           "anonymous",           false },
    };

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task Policy_Session_IsAllowedOrDenied(string policy, string session, bool allowed)
    {
        var result = await Authorization.AuthorizeAsync(Principal(session), resource: null, policy);

        Assert.Equal(allowed, result.Succeeded);
    }

    [Theory]
    [InlineData("member", true)]
    [InlineData("orgAdmin", true)]
    [InlineData("startPassword", false)]
    [InlineData("learner", false)]
    [InlineData("anonymous", false)]
    public async Task DefaultPolicy_Session_OnlySignedInAdults(string session, bool allowed)
    {
        // [Authorize] without a policy: signed-in adult without a pending password change (LP-104).
        var policy = await PolicyProvider.GetDefaultPolicyAsync();

        var result = await Authorization.AuthorizeAsync(Principal(session), resource: null, policy);

        Assert.Equal(allowed, result.Succeeded);
    }

    private static ClaimsPrincipal Principal(string session) => session switch
    {
        "anonymous" => new ClaimsPrincipal(new ClaimsIdentity()),
        "member" => Adult(AuthRoles.Member),
        "orgAdmin" => Adult(AuthRoles.OrgAdmin),
        "startPassword" => Adult(AuthRoles.Member, new Claim(AuthClaims.MustChangePassword, "true")),
        "startPasswordAdmin" => Adult(AuthRoles.OrgAdmin, new Claim(AuthClaims.MustChangePassword, "true")),
        "noRole" => Signed([new Claim(AuthClaims.Subject, "1"), new Claim(AuthClaims.Name, "Anna")]),
        "learner" => Signed(
        [
            new Claim(AuthClaims.Subject, "7"),
            new Claim(AuthClaims.Name, "Mia"),
            new Claim(AuthClaims.Role, AuthRoles.Learner),
            new Claim(AuthClaims.OrganizationId, "1"),
            new Claim(AuthClaims.LearnerId, "7"),
            new Claim(AuthClaims.DeviceId, "3"),
        ]),
        _ => throw new ArgumentOutOfRangeException(nameof(session), session, null),
    };

    private static ClaimsPrincipal Adult(string role, params Claim[] extra) => Signed(
    [
        new Claim(AuthClaims.Subject, "1"),
        new Claim(AuthClaims.Name, "Anna"),
        new Claim(AuthClaims.Role, role),
        new Claim(AuthClaims.OrganizationId, "1"),
        .. extra,
    ]);

    private static ClaimsPrincipal Signed(IEnumerable<Claim> claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Bearer", nameType: AuthClaims.Name, roleType: AuthClaims.Role));
}
