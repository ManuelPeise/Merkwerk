using System.Reflection;
using Architecture.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Architecture.Tests;

/// <summary>Hard rules of AGENTS.md §3 that the project files alone cannot show.</summary>
public sealed class LayerTests
{
    private const BindingFlags AllMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void LogicAssemblies_DoNotReference_AspNetCore()
    {
        var violations = Assemblies.Logic
            .SelectMany(a => Assemblies.ReferencedNames(a)
                .Where(n => n.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal))
                .Select(n => $"{a.GetName().Name} -> {n}"))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void LogicShared_DoesNotReference_EfCoreOrData()
    {
        var forbidden = Assemblies.ReferencedNames(Assemblies.LogicShared)
            .Where(n => n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                || n.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
                || n.StartsWith("Data.", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(forbidden);
    }

    [Fact]
    public void Shared_References_NoFrameworkOrProject()
    {
        // Plain records and enums (LP-164): no EF Core, no ASP.NET Core, no Merkwerk project.
        var forbidden = Assemblies.ReferencedNames(Assemblies.Shared)
            .Where(n => n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                || n.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
                || n.StartsWith("Data.", StringComparison.Ordinal)
                || n.StartsWith("Logic.", StringComparison.Ordinal)
                || n.StartsWith("Web.", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(forbidden);
    }

    [Fact]
    public void Enums_AreDeclaredInSharedEnums()
    {
        // All enums live in Shared.Enums (LP-164) – except compiler-generated ones.
        var violations = Assemblies.AllButShared
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsEnum && !t.Name.Contains('<', StringComparison.Ordinal))
            .Select(t => t.FullName)
            .Concat(Assemblies.Shared.GetTypes()
                .Where(t => t.IsEnum && t.Namespace != "Shared.Enums")
                .Select(t => t.FullName))
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void ServiceInterfaces_AreDeclaredInLogicSharedInterfaces()
    {
        // Public interfaces of the logic layer live in Logic.Shared.Interfaces (LP-164); repositories stay in
        // Data.Accessor.Abstractions, ICurrentUser in Data.Database.
        var violations = Assemblies.Logic
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsInterface: true, IsPublic: true })
            .Where(t => t.Namespace != "Logic.Shared.Interfaces")
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void LogicTypes_DoNotUse_DbContextOrDbSet()
    {
        // Logic reaches data only through IUnitOfWork (ADR 012); entity types and EF query extensions are fine.
        var violations = Assemblies.Logic
            .SelectMany(a => a.GetTypes())
            .SelectMany(type => MemberTypes(type).Where(IsDbAccessType).Select(t => $"{type.FullName} uses {t.Name}"))
            .Distinct()
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Controllers_InWebCore_DeriveFromApiControllerBaseInApiControllersNamespace()
    {
        const string controllerNamespace = "Web.Core.Services.ApiControllers";

        var violations = Assemblies.WebCore.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .Where(t => !t.IsSubclassOf(typeof(Web.Core.Services.ApiControllers.ApiControllerBase))
                || t.Namespace is null
                || !t.Namespace.StartsWith(controllerNamespace, StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void DataAccessor_Implementations_AreNotPublic()
    {
        // Logic sees only Data.Accessor.Abstractions; implementations stay internal (LP-102).
        var publicImplementations = Assemblies.DataAccessor.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(t => t.Namespace != "Data.Accessor.DI")
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(publicImplementations);
    }

    private static IEnumerable<Type> MemberTypes(Type type)
    {
        foreach (var field in type.GetFields(AllMembers))
        {
            yield return field.FieldType;
        }

        foreach (var property in type.GetProperties(AllMembers))
        {
            yield return property.PropertyType;
        }

        foreach (var method in type.GetMethods(AllMembers).Cast<MethodBase>().Concat(type.GetConstructors(AllMembers)))
        {
            foreach (var parameter in method.GetParameters())
            {
                yield return parameter.ParameterType;
            }
        }
    }

    private static bool IsDbAccessType(Type type)
    {
        if (typeof(DbContext).IsAssignableFrom(type))
        {
            return true;
        }

        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        return definition == typeof(DbSet<>)
            || definition == typeof(IDbContextFactory<>)
            || type.GetGenericArguments().Any(IsDbAccessType);
    }
}
