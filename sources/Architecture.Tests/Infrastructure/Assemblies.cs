using System.Reflection;

namespace Architecture.Tests.Infrastructure;

/// <summary>The production assemblies, loaded through a type each project is known to contain.</summary>
internal static class Assemblies
{
    public static readonly Assembly WebCore = typeof(Web.Core.Services.ApiControllers.ApiControllerBase).Assembly;

    public static readonly Assembly LogicAuthentication = typeof(Logic.Authentication.TokenService).Assembly;

    public static readonly Assembly LogicShared = typeof(Logic.Shared.DI.ServiceRegistrationExtensions).Assembly;

    public static readonly Assembly DataAccessor = typeof(Data.Accessor.Abstractions.IUnitOfWork).Assembly;

    /// <summary>All Logic.* assemblies. Add new Logic projects here (the project rule test reminds you).</summary>
    public static readonly IReadOnlyList<Assembly> Logic = [LogicAuthentication, LogicShared];

    public static IEnumerable<string> ReferencedNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);
}
