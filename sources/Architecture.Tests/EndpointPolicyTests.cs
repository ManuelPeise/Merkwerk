using System.Reflection;
using Architecture.Tests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Core.Services.Authorization;

namespace Architecture.Tests;

/// <summary>
/// LP-107: the permission matrix (AGENTS.md §9) per endpoint. Changing a policy means changing this table – on purpose,
/// so nobody opens or closes an endpoint by accident.
/// </summary>
public sealed class EndpointPolicyTests
{
    private const string Anonymous = "(anonymous)";

    /// <summary>Controller.Action (without "Controller"/"Async") → policy, or <see cref="Anonymous"/>.</summary>
    private static readonly IReadOnlyDictionary<string, string> Expected = new Dictionary<string, string>
    {
        ["Authentication.Login"] = Anonymous,
        ["Authentication.Refresh"] = Anonymous,
        ["Authentication.Logout"] = Anonymous,
        ["Authentication.ForgotPassword"] = Anonymous,
        ["Authentication.ResetPassword"] = Anonymous,
        ["Authentication.ConfirmEmail"] = Anonymous,
        ["Authentication.Me"] = AuthorizationPolicies.AnySession,
        ["Authentication.ChangePassword"] = AuthorizationPolicies.PasswordChangeAllowed,
        ["Authentication.AdminResetPassword"] = AuthorizationPolicies.OrgAdmin,

        ["Setup.Status"] = Anonymous,
        ["Setup.Initialize"] = Anonymous,

        ["Invitations.Details"] = Anonymous,
        ["Invitations.Accept"] = Anonymous,
        ["Invitations.Create"] = AuthorizationPolicies.OrgAdmin,
        ["Invitations.List"] = AuthorizationPolicies.OrgAdmin,
        ["Invitations.Revoke"] = AuthorizationPolicies.OrgAdmin,

        ["Members.List"] = AuthorizationPolicies.Member,
        ["Members.Remove"] = AuthorizationPolicies.OrgAdmin,

        ["Learners.List"] = AuthorizationPolicies.Member,
        ["Learners.Create"] = AuthorizationPolicies.OrgAdmin,
        ["Learners.Update"] = AuthorizationPolicies.OrgAdmin,
        ["Learners.Delete"] = AuthorizationPolicies.OrgAdmin,

        ["Groups.List"] = AuthorizationPolicies.Member,
        ["Groups.Create"] = AuthorizationPolicies.OrgAdmin,
        ["Groups.Update"] = AuthorizationPolicies.OrgAdmin,
        ["Groups.Delete"] = AuthorizationPolicies.OrgAdmin,

        // Subjects are instance-wide; the family's admin maintains them (LP-109, later the instance admin).
        ["Subjects.List"] = AuthorizationPolicies.Member,
        ["Subjects.Create"] = AuthorizationPolicies.OrgAdmin,
        ["Subjects.Update"] = AuthorizationPolicies.OrgAdmin,

        // Every adult of the family works on exercises (LP-110).
        ["Exercises.List"] = AuthorizationPolicies.Member,
        ["Exercises.Get"] = AuthorizationPolicies.Member,
        ["Exercises.Version"] = AuthorizationPolicies.Member,
        ["Exercises.Create"] = AuthorizationPolicies.Member,
        ["Exercises.Update"] = AuthorizationPolicies.Member,
        ["Exercises.Publish"] = AuthorizationPolicies.Member,
        ["Exercises.Archive"] = AuthorizationPolicies.Member,

        // Every adult of the family may pair and unpair devices (decision 04.10.2026).
        ["Devices.PairingCode"] = AuthorizationPolicies.Member,
        ["Devices.List"] = AuthorizationPolicies.Member,
        ["Devices.Revoke"] = AuthorizationPolicies.Member,

        // The device itself (cookie mw_device), not a signed-in person.
        ["Devices.Pair"] = Anonymous,
        ["Devices.Status"] = Anonymous,
        ["Devices.Profiles"] = Anonymous,
        ["Devices.SignIn"] = Anonymous,
    };

    [Fact]
    public void EveryAction_HasExactlyOneExplicitPolicyOrAllowAnonymous()
    {
        var violations = Actions()
            .Select(a => (a.Name, Problem: Describe(a.Method)))
            .Where(a => a.Problem is not null)
            .Select(a => $"{a.Name}: {a.Problem}")
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void EveryAction_HasThePolicyOfThePermissionMatrix()
    {
        var actual = Actions().ToDictionary(a => a.Name, a => PolicyOf(a.Method));

        var wrong = Expected
            .Where(e => !actual.TryGetValue(e.Key, out var policy) || policy != e.Value)
            .Select(e => $"{e.Key}: expected {e.Value}, found {(actual.TryGetValue(e.Key, out var p) ? p : "no such action")}");
        var unlisted = actual.Keys.Except(Expected.Keys).Select(k => $"{k}: not in the permission matrix");

        Assert.Empty(wrong.Concat(unlisted));
    }

    [Fact]
    public void Controllers_HaveNoClassLevelAuthorization()
    {
        // Class-level attributes would combine with the action's and blur the matrix.
        var violations = Controllers()
            .Where(c => c.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any()
                || c.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any())
            .Select(c => c.Name)
            .ToList();

        Assert.Empty(violations);
    }

    private static string? Describe(MethodInfo method)
    {
        var authorize = method.GetCustomAttributes<AuthorizeAttribute>().ToList();
        var anonymous = method.GetCustomAttributes<AllowAnonymousAttribute>().Any();

        return (authorize.Count, anonymous) switch
        {
            (0, false) => "neither [Authorize(Policy = …)] nor [AllowAnonymous]",
            (> 0, true) => "[Authorize] and [AllowAnonymous] together",
            (> 1, _) => "more than one [Authorize]",
            (1, false) when !string.IsNullOrEmpty(authorize[0].Roles) => "uses Roles = … (skips the start-password check)",
            (1, false) when string.IsNullOrEmpty(authorize[0].Policy) => "[Authorize] without a policy",
            _ => null,
        };
    }

    private static string PolicyOf(MethodInfo method) =>
        method.GetCustomAttributes<AllowAnonymousAttribute>().Any()
            ? Anonymous
            : method.GetCustomAttribute<AuthorizeAttribute>()?.Policy ?? "(none)";

    private static IEnumerable<Type> Controllers() => Assemblies.WebCore.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t));

    private static IEnumerable<(string Name, MethodInfo Method)> Actions() => Controllers()
        .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() is null)
            .Select(m => ($"{Trim(c.Name, "Controller")}.{Trim(m.Name, "Async")}", m)));

    private static string Trim(string value, string suffix) =>
        value.EndsWith(suffix, StringComparison.Ordinal) ? value[..^suffix.Length] : value;
}
