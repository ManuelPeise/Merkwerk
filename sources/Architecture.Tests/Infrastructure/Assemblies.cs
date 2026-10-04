using System.Reflection;

namespace Architecture.Tests.Infrastructure;

/// <summary>The production assemblies, loaded through a type each project is known to contain.</summary>
internal static class Assemblies
{
    public static readonly Assembly WebCore = typeof(Web.Core.Services.ApiControllers.ApiControllerBase).Assembly;

    public static readonly Assembly LogicAuthentication = typeof(Logic.Authentication.TokenService).Assembly;

    public static readonly Assembly LogicContent = typeof(Logic.Content.Subjects.SubjectRules).Assembly;

    public static readonly Assembly LogicDevices = typeof(Logic.Devices.DI.ServiceCollectionExtensions).Assembly;

    public static readonly Assembly LogicNotifications = typeof(Logic.Notifications.MailOptions).Assembly;

    public static readonly Assembly LogicOrganizations = typeof(Logic.Organizations.PrivacyPolicy).Assembly;

    public static readonly Assembly LogicShared = typeof(Logic.Shared.DI.ServiceRegistrationExtensions).Assembly;

    public static readonly Assembly DataAccessor = typeof(Data.Accessor.Abstractions.IUnitOfWork).Assembly;

    public static readonly Assembly DataDatabase = typeof(Data.Database.MerkwerkDbContext).Assembly;

    /// <summary>Service models and enums (LP-164).</summary>
    public static readonly Assembly Shared = typeof(global::Shared.Enums.OrganizationRole).Assembly;

    /// <summary>All Logic.* assemblies. Add new Logic projects here (the project rule test reminds you).</summary>
    public static readonly IReadOnlyList<Assembly> Logic = [LogicAuthentication, LogicContent, LogicDevices, LogicNotifications, LogicOrganizations, LogicShared];

    /// <summary>Every production assembly except Shared itself.</summary>
    public static readonly IReadOnlyList<Assembly> AllButShared = [.. Logic, DataAccessor, DataDatabase, WebCore];

    public static IEnumerable<string> ReferencedNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);
}
